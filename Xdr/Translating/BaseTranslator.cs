using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.ReadContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();
		
		protected BaseTranslator()
		{
		}
		
		protected void Init()
		{
			SetReadOne<int>(ReadInt32);
			SetReadOne<uint>(ReadUInt32);
			SetReadOne<long>(ReadInt64);
			SetReadOne<ulong>(ReadUInt64);
			SetReadOne<float>(ReadSingle);
			SetReadOne<double>(ReadDouble);
			SetReadOne<bool>(ReadBool);
			SetReadFix<byte[]>(ReadFixBytes);
			SetReadVar<byte[]>(ReadVarBytes);
			SetReadVar<string>(ReadString);
			
			SetWriteOne<int>(WriteInt32);
			SetWriteOne<uint>(WriteUInt32);
			SetWriteOne<long>(WriteInt64);
			SetWriteOne<ulong>(WriteUInt64);
			SetWriteOne<float>(WriteSingle);
			SetWriteOne<double>(WriteDouble);
			SetWriteOne<bool>(WriteBool);
			SetWriteFix<byte[]>(WriteFixBytes);
			SetWriteVar<byte[]>(WriteVarBytes);
			SetWriteVar<string>(WriteString);
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
					
					BuildMethod(bReq.TargetType, bReq.Method);
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
			switch(methodType)
			{
			case MethodType.ReadOne:
				TypedAppendMethod(GetReadOneCacheType, ReadOneBuild, targetType, method);
				break;
			case MethodType.ReadFix:
				TypedAppendMethod(GetReadFixCacheType, ReadFixBuild, targetType, method);
				break;
			case MethodType.ReadVar:
				TypedAppendMethod(GetReadVarCacheType, ReadVarBuild, targetType, method);
				break;
			case MethodType.WriteOne:
				TypedAppendMethod(GetWriteOneCacheType, WriteOneBuild, targetType, method);
				break;
			case MethodType.WriteFix: 
				TypedAppendMethod(GetWriteFixCacheType, WriteFixBuild, targetType, method);
				break;
			case MethodType.WriteVar:
				TypedAppendMethod(GetWriteVarCacheType, WriteVarBuild, targetType, method);
				break;
			default:
				throw new NotImplementedException("unknown method type");
			}
		}
		
		private static void TypedAppendMethod(Func<Type> getType, Func<Type, Delegate> build, Type targetType, Delegate method)
		{
			FieldInfo fi = getType().MakeGenericType(targetType).GetField("Instance");
			if(fi.GetValue(null) != null)
				throw new InvalidOperationException("type already mapped");
			
			fi.SetValue(null, method);
		}
		
		private void BuildMethod(Type targetType, MethodType methodType)
		{
			switch(methodType)
			{
			case MethodType.ReadOne:
				TypedBuildMethod(GetReadOneCacheType, ReadOneBuild, targetType);
				break;
			case MethodType.ReadFix:
				TypedBuildMethod(GetReadFixCacheType, ReadFixBuild, targetType);
				break;
			case MethodType.ReadVar:
				TypedBuildMethod(GetReadVarCacheType, ReadVarBuild, targetType);
				break;
			case MethodType.WriteOne:
				TypedBuildMethod(GetWriteOneCacheType, WriteOneBuild, targetType);
				break;
			case MethodType.WriteFix: 
				TypedBuildMethod(GetWriteFixCacheType, WriteFixBuild, targetType);
				break;
			case MethodType.WriteVar:
				TypedBuildMethod(GetWriteVarCacheType, WriteVarBuild, targetType);
				break;
			default:
				throw new NotImplementedException("unknown method type");
			}
		}
		
		private static void TypedBuildMethod(Func<Type> getType, Func<Type, Delegate> build, Type targetType)
		{
			FieldInfo fi = getType().MakeGenericType(targetType).GetField("Instance");
			if(fi.GetValue(null) == null)
				fi.SetValue(null, build(targetType));
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
