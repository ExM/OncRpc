using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.EmitContexts;

namespace Xdr
{
	public abstract class WriteMapper
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();

		private Dictionary<OpaqueType, Func<Type, Delegate>[]> _builders = new Dictionary<OpaqueType, Func<Type, Delegate>[]>();

		protected WriteMapper()
		{
		}
		
		protected void Init()
		{
			SetOne<Void>((w, i) => { });
			SetOne<int>((w, i) => XdrEncoding.EncodeInt32(i, w.ByteWriter));
			SetOne<uint>((w, i) => XdrEncoding.EncodeUInt32(i, w.ByteWriter));
			SetOne<long>((w, i) => XdrEncoding.EncodeInt64(i, w.ByteWriter));
			SetOne<ulong>((w, i) => XdrEncoding.EncodeUInt64(i, w.ByteWriter));
			SetOne<float>((w, i) => XdrEncoding.EncodeSingle(i, w.ByteWriter));
			SetOne<double>((w, i) => XdrEncoding.EncodeDouble(i, w.ByteWriter));
			SetOne<bool>(WriteBool);
			SetFix<byte[]>(WriteFixOpaque);
			SetVar<byte[]>(WriteVarOpaque);
			SetVar<string>(WriteString);

			_builders.Add(OpaqueType.One,
				new Func<Type, Delegate>[] { CreateEnumWriter, CreateNullableWriter, CreateLinkedListWriter, EmitContext.GetWriter });
			_builders.Add(OpaqueType.Fix,
				new Func<Type, Delegate>[] { CreateFixArrayWriter, CreateFixListWriter });
			_builders.Add(OpaqueType.Var,
				new Func<Type, Delegate>[] { CreateVarArrayWriter, CreateVarListWriter });
		}

		private static void WriteBool(Writer w, bool v)
		{
			w.Write<int>(v?1:0);
		}
		
		private static byte[][] _tails = new byte[][]
		{
			null,
			new byte[] { 0x00},
			new byte[] { 0x00, 0x00},
			new byte[] { 0x00, 0x00, 0x00}
		};

		private static void WriteFixOpaque(Writer w, uint len, byte[] v)
		{
			if(v.LongLength != len)
				throw new FormatException("unexpected length: " + v.LongLength.ToString());

			NoCheckWriteFixOpaque(w, len, v);
		}

		private static void WriteVarOpaque(Writer w, uint max, byte[] v)
		{
			uint len = (uint)v.LongLength;
			if(len > max)
				throw new FormatException("unexpected length: " + len.ToString());

			try
			{
				w.Write<uint>(len);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write length", ex);
			}
			NoCheckWriteFixOpaque(w, len, v);
		}
		
		private static void NoCheckWriteFixOpaque(Writer w, uint len, byte[] v)
		{
			try
			{
				w.ByteWriter.Write(v);
				uint tail = len % 4u;
				if (tail != 0)
					w.ByteWriter.Write(_tails[4u - tail]);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write byte array", ex);
			}
		}
		
		private static void WriteString(Writer w, uint max, string v)
		{
			WriteVarOpaque(w, max, Encoding.ASCII.GetBytes(v));
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
				return ErrorStub.WriteOneDelegate(targetType, wrap);
			else
				return ErrorStub.WriteManyDelegate(targetType, wrap);
		}
		
		protected void SetOne<T>(WriteOneDelegate<T> method)
		{
			GetOneCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}
		
		protected void SetFix<T>(WriteManyDelegate<T> method)
		{
			GetFixCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		protected void SetVar<T>(WriteManyDelegate<T> method)
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

		public static Delegate CreateFixArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteFixArray").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static void WriteFixArray<T>(Writer w, uint len, T[] val)
		{
			if(val.LongLength != len)
				throw new FormatException("unexpected length: " + val.LongLength.ToString());

			uint i = 0;
			try
			{
				for (; i < len; i++)
					w.Write<T>(val[i]);
			}
			catch (SystemException ex)
			{
				throw new FormatException(string.Format("can't write {0} item", i), ex);
			}
		}

		public static Delegate CreateLinkedListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteLinkedList").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static void WriteLinkedList<T>(Writer w, List<T> val)
		{
			for (int i = 0; i < val.Count; i++)
			{
				WriteOption(w, true);
				try
				{
					w.Write<T>(val[i]);
				}
				catch (SystemException ex)
				{
					throw new FormatException(string.Format("can't write {0} item", i), ex);
				}
			}
			WriteOption(w, false);
		}

		public static Delegate CreateFixListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteFixList").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static void WriteFixList<T>(Writer w, uint len, List<T> val)
		{
			if(val.Count != len)
				throw new FormatException("unexpected length: " + val.Count.ToString());

			int i = 0;
			try
			{
				for (; i < val.Count; i++)
					w.Write<T>(val[i]);
			}
			catch (SystemException ex)
			{
				throw new FormatException(string.Format("can't write {0} item", i), ex);
			}
		}

		public static Delegate CreateEnumWriter(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(WriteMapper).GetMethod("EnumWriter").MakeGenericMethod(targetType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static void EnumWriter<T>(Writer writer, T val) where T : struct
		{
			writer.Write<int>(EnumHelper<T>.EnumToInt(val));
		}

		public static Delegate CreateNullableWriter(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteNullable").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static void WriteNullable<T>(Writer writer, T? val) where T : struct
		{
			WriteOption(writer, val.HasValue);

			if (!val.HasValue)
				return;

			try
			{
				writer.Write<T>(val.Value);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write value", ex);
			}
		}

		private static void WriteOption(Writer writer, bool val)
		{
			try
			{
				writer.Write<bool>(val);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write option", ex);
			}
		}

		public static Delegate CreateVarArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteVarArray").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static void WriteVarArray<T>(Writer w, uint max, T[] val)
		{
			uint len = (uint)val.LongLength;
			if (len > max)
				throw new FormatException("unexpected length: " + len.ToString());

			try
			{
				w.Write<uint>(len);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write length", ex);
			}

			uint i = 0;
			try
			{
				for (; i < len; i++)
					w.Write<T>(val[i]);
			}
			catch (SystemException ex)
			{
				throw new FormatException(string.Format("can't write {0} item", i), ex);
			}
		}

		public static Delegate CreateVarListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(WriteMapper).GetMethod("WriteVarList").MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static void WriteVarList<T>(Writer w, uint max, List<T> val)
		{
			int len = val.Count;
			if (len > max)
				throw new FormatException("unexpected length: " + len.ToString());

			try
			{
				w.Write<uint>((uint)len);
			}
			catch (SystemException ex)
			{
				throw new FormatException("can't write length", ex);
			}

			int i = 0;
			try
			{
				for (; i < len; i++)
					w.Write<T>(val[i]);
			}
			catch (SystemException ex)
			{
				throw new FormatException(string.Format("can't write {0} item", i), ex);
			}
		}
	}
}
