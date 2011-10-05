using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr2
{
	public abstract class ReadMapper
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();

		private Dictionary<OpaqueType, Func<Type, Delegate>[]> _builders = new Dictionary<OpaqueType, Func<Type, Delegate>[]>();

		protected ReadMapper()
		{
		}
		
		protected void Init()
		{
			SetOne<int>((r) => XdrEncoding.DecodeInt32(r.ByteReader));
			SetOne<uint>((r) => XdrEncoding.DecodeUInt32(r.ByteReader));
			SetOne<long>((r) => XdrEncoding.DecodeInt64(r.ByteReader));
			SetOne<ulong>((r) => XdrEncoding.DecodeUInt64(r.ByteReader));
			SetOne<float>((r) => XdrEncoding.DecodeSingle(r.ByteReader));
			SetOne<double>((r) => XdrEncoding.DecodeDouble(r.ByteReader));
			SetOne<bool>(ReadBool);
			SetFix<byte[]>(ReadFixOpaque);
			SetVar<byte[]>(ReadVarOpaque);

			_builders.Add(OpaqueType.One,
				new Func<Type, Delegate>[] { CreateEnumReader, CreateNullableReader /*, EmitContext.GetReader for attribute*/ });
			_builders.Add(OpaqueType.Fix,
				new Func<Type, Delegate>[] { CreateFixArrayReader, CreateFixListReader });
			_builders.Add(OpaqueType.Var,
				new Func<Type, Delegate>[] { CreateVarArrayReader, CreateVarListReader });
		}

		private static bool ReadBool(Reader r)
		{
			uint val = XdrEncoding.DecodeUInt32(r.ByteReader);
			if (val == 0)
				return false;
			if (val == 1)
				return true;

			throw new InvalidOperationException("unexpected value");
		}

		private static byte[] ReadFixOpaque(Reader r, uint len)
		{
			byte[] result = r.ByteReader.Read(len);
			uint tail = len % 4u;
			if (tail != 0)
				r.ByteReader.Read(4u - tail);
			return result;
		}

		private static byte[] ReadVarOpaque(Reader r, uint max)
		{
			uint len = XdrEncoding.DecodeUInt32(r.ByteReader);
			if(len > max)
				throw new InvalidOperationException("unexpected length");

			return ReadFixOpaque(r, len);
		}

		private Delegate BuildDelegate(OpaqueType methodType, Type targetType)
		{
			Exception wrap = null;

			try
			{
				foreach (var build in _builders[methodType])
				{
					Delegate result = build(targetType);
					if (result != null)
						return result;
				}
			}
			catch (Exception ex)
			{
				wrap = new InvalidOperationException(
					string.Format("impossible to create a {0} method type for `{1}'", methodType, targetType.FullName), ex);
			}

			if(wrap == null)
				wrap = new NotImplementedException(string.Format("unknown type `{0}' in {1} method type", targetType.FullName, methodType));
			
			if(methodType == OpaqueType.One)
				return ErrorStub.ReadOneDelegate(targetType, wrap);
			else
				return ErrorStub.ReadManyDelegate(targetType, wrap);
		}
		
		protected void SetOne<T>(ReadOneDelegate<T> method)
		{
			GetOneCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}
		
		protected void SetFix<T>(ReadManyDelegate<T> method)
		{
			GetFixCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		protected void SetVar<T>(ReadManyDelegate<T> method)
		{
			GetVarCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		private Type GetCacheType(OpaqueType methodType)
		{
			switch (methodType)
			{
				case OpaqueType.One: return GetOneCacheType();
				case OpaqueType.Fix: return GetFixCacheType();
				case OpaqueType.Var: return GetVarCacheType();
				default:
					throw new NotImplementedException("unknown opaque type");
			}
		}
		
		protected abstract Type GetOneCacheType();
		
		protected abstract Type GetFixCacheType();

		protected abstract Type GetVarCacheType();

		public void BuildCaches()
		{
			lock (_sync)
			{
				while(true)
				{
					BuildRequest bReq = null;
					lock(_dependencySync)
						if (_dependency.Count != 0)
							bReq = _dependency.Dequeue();
					if (bReq == null)
						return;

					FieldInfo fi = GetCacheType(bReq.Method).MakeGenericType(bReq.TargetType).GetField("Instance");
					if (fi.GetValue(null) == null)
						fi.SetValue(null, BuildDelegate(bReq.Method, bReq.TargetType));
				}
			}
		}

		internal void AppendMethod(Type targetType, OpaqueType methodType, Delegate method)
		{
			lock(_sync)
				LockedAppendMethod(targetType, methodType, method);
		}

		private void LockedAppendMethod(Type targetType, OpaqueType methodType, Delegate method)
		{
			FieldInfo fi = GetCacheType(methodType).MakeGenericType(targetType).GetField("Instance");
			if (fi.GetValue(null) != null)
				throw new InvalidOperationException("type already mapped");

			fi.SetValue(null, method);
		}

		protected void AppendBuildRequest(Type targetType, OpaqueType methodType)
		{
			lock (_dependencySync)
				_dependency.Enqueue(new BuildRequest { TargetType = targetType, Method = methodType });
		}

		public static Delegate CreateFixArrayReader(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ReadMapper).GetMethod("ReadFixArray").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static T[] ReadFixArray<T>(Reader r, uint len)
		{
			T[] result = new T[len];
			for (uint i = 0; i < len; i++)
				result[i] = r.Read<T>();
			return result;
		}

		public static Delegate CreateFixListReader(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ReadMapper).GetMethod("ReadFixList").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static List<T> ReadFixList<T>(Reader r, uint len)
		{
			List<T> result = new List<T>();
			for (uint i = 0; i < len; i++)
				result.Add(r.Read<T>());
			return result;
		}

		public static Delegate CreateEnumReader(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(ReadMapper).GetMethod("EnumRead").MakeGenericMethod(targetType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static T EnumRead<T>(Reader reader) where T : struct
		{
			return EnumHelper<T>.IntToEnum(reader.Read<int>());
		}

		public static Delegate CreateNullableReader(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ReadMapper).GetMethod("ReadNullable").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static T? ReadNullable<T>(Reader reader) where T : struct
		{
			if (reader.Read<bool>())
				return reader.Read<T>();
			else
				return null;
		}

		public static Delegate CreateVarArrayReader(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ReadMapper).GetMethod("ReadVarArray").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static T[] ReadVarArray<T>(Reader r, uint max)
		{
			uint len = r.Read<uint>();
			if (len > max)
				throw new InvalidOperationException("unexpected Length");

			return ReadFixArray<T>(r, len);
		}

		public static Delegate CreateVarListReader(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ReadMapper).GetMethod("ReadVarList").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static List<T> ReadVarList<T>(Reader r, uint max)
		{
			uint len = r.Read<uint>();
			if (len > max)
				throw new InvalidOperationException("unexpected Length");

			return ReadFixList<T>(r, len);
		}
	}
}
