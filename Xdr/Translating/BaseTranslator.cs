using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.Contexts;
using Xdr.EmitContexts;

namespace Xdr
{
	public abstract class BaseTranslator: ITranslator
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();

		private Dictionary<MethodType, Func<Type, Delegate>[]> _builders = new Dictionary<MethodType, Func<Type, Delegate>[]>();

		protected BaseTranslator()
		{
		}
		
		protected void Init()
		{
			SetReadOne<int>((r, c, e) => r.ReadInt32(c, e));
			SetReadOne<uint>((r, c, e) => r.ReadUInt32(c, e));
			SetReadOne<long>((r, c, e) => r.ReadInt64(c, e));
			SetReadOne<ulong>((r, c, e) => r.ReadUInt64(c, e));
			SetReadOne<float>((r, c, e) => r.ReadSingle(c, e));
			SetReadOne<double>((r, c, e) => r.ReadDouble(c, e));
			SetReadOne<bool>((r, c, e) => r.ReadInt32((val) => IntToBool(val, c, e), e));
			SetReadFix<byte[]>((r, l, c, e) => r.ReadFixOpaque(l, c, e));
			SetReadVar<byte[]>((r, l, c, e) => r.ReadVarOpaque(l, c, e));
			SetReadVar<string>((r, l, c, e) => r.ReadString(l, c, e));
			
			SetWriteOne<int>((w, i, c, e) => w.WriteInt32(i, c, e));
			SetWriteOne<uint>((w, i, c, e) => w.WriteUInt32(i, c, e));
			SetWriteOne<long>((w, i, c, e) => w.WriteInt64(i, c, e));
			SetWriteOne<ulong>((w, i, c, e) => w.WriteUInt64(i, c, e));
			SetWriteOne<float>((w, i, c, e) => w.WriteSingle(i, c, e));
			SetWriteOne<double>((w, i, c, e) => w.WriteDouble(i, c, e));
			SetWriteOne<bool>((w, i, c, e) => w.WriteInt32(i?1:0, c, e));
			SetWriteFix<byte[]>((w, i, l, c, e) => w.WriteFixOpaque(i, l, c, e));
			SetWriteVar<byte[]>((w, i, l, c, e) => w.WriteVarOpaque(i, l, c, e));
			SetWriteVar<string>((w, i, l, c, e) => w.WriteString(i, l, c, e));

			_builders.Add(MethodType.ReadOne,
				new Func<Type, Delegate>[] { CreateEnumReader, CreateNullableReader, EmitContext.GetReader });
			_builders.Add(MethodType.ReadFix,
				new Func<Type, Delegate>[] { CreateFixArrayReader, CreateFixListReader });
			_builders.Add(MethodType.ReadVar,
				new Func<Type, Delegate>[] { CreateVarArrayReader, CreateVarListReader });

			_builders.Add(MethodType.WriteOne,
				new Func<Type, Delegate>[] { CreateEnumWriter, CreateNullableWriter, EmitContext.GetWriter });
			_builders.Add(MethodType.WriteFix,
				new Func<Type, Delegate>[] { CreateFixArrayWriter, CreateFixListWriter });
			_builders.Add(MethodType.WriteVar,
				new Func<Type, Delegate>[] { CreateVarArrayWriter, CreateVarListWriter });
		}

		private Delegate BuildDelegate(MethodType methodType, Type targetType)
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

			switch (methodType)
			{
				case MethodType.ReadOne:
					return ErrorStub.ReadOneDelegate(targetType, wrap);
				case MethodType.ReadFix:
				case MethodType.ReadVar:
					return ErrorStub.ReadManyDelegate(targetType, wrap);

				case MethodType.WriteOne:
					return ErrorStub.WriteOneDelegate(targetType, wrap);
				case MethodType.WriteFix:
				case MethodType.WriteVar:
				default:
					return ErrorStub.WriteManyDelegate(targetType, wrap);
			}
		}
		
		private static void IntToBool(int val, Action<bool> completed, Action<Exception> excepted)
		{
			if (val == 0)
				completed(false);
			else if(val == 1)
				completed(true);
			else
				excepted(new InvalidCastException(string.Format("no boolean value `{0}'", val)));
		}
		
		protected void SetReadOne<T>(ReadOneDelegate<T> method)
		{
			GetReadOneCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}
		
		protected void SetReadFix<T>(ReadManyDelegate<T> method)
		{
			GetReadFixCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		protected void SetReadVar<T>(ReadManyDelegate<T> method)
		{
			GetReadVarCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}
		
		protected void SetWriteOne<T>(WriteOneDelegate<T> method)
		{
			GetWriteOneCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}
		
		protected void SetWriteFix<T>(WriteManyDelegate<T> method)
		{
			GetWriteFixCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		protected void SetWriteVar<T>(WriteManyDelegate<T> method)
		{
			GetWriteVarCacheType()
				.MakeGenericType(typeof(T))
				.GetField("Instance")
				.SetValue(null, method);
		}

		private Type GetCacheType(MethodType methodType)
		{
			switch (methodType)
			{
				case MethodType.ReadOne: return GetReadOneCacheType();
				case MethodType.ReadFix: return GetReadFixCacheType();
				case MethodType.ReadVar: return GetReadVarCacheType();
				case MethodType.WriteOne: return GetWriteOneCacheType();
				case MethodType.WriteFix: return GetWriteFixCacheType();
				case MethodType.WriteVar: return GetWriteVarCacheType();
				default:
					throw new NotImplementedException("unknown method type");
			}
		}
		
		protected abstract Type GetReadOneCacheType();
		public abstract void Read<T>(Reader reader, Action<T> completed, Action<Exception> excepted);
		
		protected abstract Type GetReadFixCacheType();
		public abstract void ReadFix<T>(Reader reader, uint len, Action<T> completed, Action<Exception> excepted);

		protected abstract Type GetReadVarCacheType();
		public abstract void ReadVar<T>(Reader reader, uint max, Action<T> completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteOneCacheType();
		public abstract void Write<T>(Writer writer, T item, Action completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteFixCacheType();
		public abstract void WriteFix<T>(Writer writer, T items, uint len, Action completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteVarCacheType();
		public abstract void WriteVar<T>(Writer writer, T items, uint max, Action completed, Action<Exception> excepted);

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
