using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.ReadContexts;
using Xdr.EmitContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
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
				new Func<Type, Delegate>[] { CreateEnumDelegate, CreateNullableReader, EmitContext.GetReader });
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
	}
}
