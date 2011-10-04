using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.Contexts;
using Xdr.EmitContexts;

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
			SetOne<int>((r) => XdrEncoding.DecodeInt32(r.ByteReader.Read(4)));
			SetOne<uint>((r, c, e) => r.ReadUInt32(c, e));
			SetOne<long>((r, c, e) => r.ReadInt64(c, e));
			SetOne<ulong>((r, c, e) => r.ReadUInt64(c, e));
			SetOne<float>((r, c, e) => r.ReadSingle(c, e));
			SetOne<double>((r, c, e) => r.ReadDouble(c, e));
			SetOne<bool>((r, c, e) => r.ReadInt32((val) => IntToBool(val, c, e), e));
			SetFix<byte[]>((r, l, c, e) => r.ReadFixOpaque(l, c, e));
			SetVar<byte[]>((r, l, c, e) => r.ReadVarOpaque(l, c, e));

			_builders.Add(OpaqueType.One,
				new Func<Type, Delegate>[] { CreateEnumReader, CreateNullableReader, EmitContext.GetReader });
			_builders.Add(OpaqueType.Fix,
				new Func<Type, Delegate>[] { CreateFixArrayReader, CreateFixListReader });
			_builders.Add(OpaqueType.Var,
				new Func<Type, Delegate>[] { CreateVarArrayReader, CreateVarListReader });
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
		
		private Type GetCacheType(MethodType methodType)
		{
			switch (methodType)
			{
				case MethodType.ReadOne: return GetOneCacheType();
				case MethodType.ReadFix: return GetFixCacheType();
				case MethodType.ReadVar: return GetVarCacheType();
				default:
					throw new NotImplementedException("unknown method type");
			}
		}
		
		protected abstract Type GetOneCacheType();
		
		protected abstract Type GetFixCacheType();

		protected abstract Type GetVarCacheType();

		protected void BuildCaches()
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
		
		internal void AppendMethod(Type targetType, MethodType methodType, Delegate method)
		{
			lock(_sync)
				LockedAppendMethod(targetType, methodType, method);
		}
		
		private void LockedAppendMethod(Type targetType, MethodType methodType, Delegate method)
		{
			FieldInfo fi = GetCacheType(methodType).MakeGenericType(targetType).GetField("Instance");
			if (fi.GetValue(null) != null)
				throw new InvalidOperationException("type already mapped");

			fi.SetValue(null, method);
		}

		protected void AppendBuildRequest(Type targetType, MethodType methodType)
		{
			lock (_dependencySync)
				_dependency.Enqueue(new BuildRequest { TargetType = targetType, Method = methodType });
		}

		public Reader CreateReader(IByteReader reader)
		{
			return new Reader(this, reader);
		}

		public Writer CreateWriter(IByteWriter writer)
		{
			return new Writer(this, writer);
		}

		public static Delegate CreateFixArrayReader(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;

			MethodInfo mi = typeof(ArrayReader<>).MakeGenericType(itemType).GetMethod("ReadFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateFixListReader(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;

			Type genericType = collectionType.GetGenericTypeDefinition();
			if (genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];

			MethodInfo mi = typeof(ListReader<>).MakeGenericType(itemType).GetMethod("ReadFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateEnumReader(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(BaseTranslator).GetMethod("EnumRead", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(targetType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void EnumRead<T>(Reader reader, Action<T> completed, Action<Exception> excepted) where T : struct
		{
			reader.ReadInt32((val) => EnumHelper<T>.IntToEnum(val, completed, excepted), excepted);
		}

		public static Delegate CreateNullableReader(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(BaseTranslator).GetMethod("ReadNullable", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void ReadNullable<T>(Reader reader, Action<T?> completed, Action<Exception> excepted)
			where T : struct
		{
			reader.ReadUInt32((val) =>
			{
				if (val == 0)
					completed(null);
				else if (val == 1)
					reader.Read<T>((item) => completed(item), excepted);
				else
					excepted(new InvalidOperationException(string.Format("unexpected value {0}", val)));
			}, excepted);
		}

		public static Delegate CreateVarArrayReader(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;

			MethodInfo mi = typeof(ArrayReader<>).MakeGenericType(itemType).GetMethod("ReadVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateVarListReader(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;

			Type genericType = collectionType.GetGenericTypeDefinition();
			if (genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];

			MethodInfo mi = typeof(ListReader<>).MakeGenericType(itemType).GetMethod("ReadVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateFixListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateFixArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateEnumWriter(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;

			MethodInfo mi = typeof(BaseTranslator).GetMethod("EnumWrite", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(targetType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void EnumWrite<T>(Writer writer, T item, Action completed, Action<Exception> excepted) where T : struct
		{
			EnumHelper<T>.EnumToInt(item, (val) => writer.WriteInt32(val, completed, excepted), excepted);
		}

		public static Delegate CreateNullableWriter(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(BaseTranslator).GetMethod("WriteNullable", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void WriteNullable<T>(Writer writer, T? item, Action completed, Action<Exception> excepted) where T : struct
		{
			if (item.HasValue)
				writer.WriteUInt32(1, () => writer.Write<T>(item.Value, completed, excepted), excepted);
			else
				writer.WriteUInt32(0, completed, excepted);
		}

		public static Delegate CreateVarListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateVarArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}
	}
}
