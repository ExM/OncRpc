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

		protected abstract Type GetReadOneCacheType();
		public abstract void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted);
		
		protected abstract Type GetReadFixCacheType();
		public abstract void ReadFix<T>(IReader reader, uint len, Action<T> completed, Action<Exception> excepted);

		protected abstract Type GetReadVarCacheType();
		public abstract void ReadVar<T>(IReader reader, uint max, Action<T> completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteOneCacheType();
		public abstract void Write<T>(IWriter writer, T item, Action completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteFixCacheType();
		public abstract void WriteFix<T>(IWriter writer, T items, uint len, Action completed, Action<Exception> excepted);
		
		protected abstract Type GetWriteVarCacheType();
		public abstract void WriteVar<T>(IWriter writer, T items, uint max, Action completed, Action<Exception> excepted);

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
				BuildMethod(targetType, methodType, method);
		}
		
		private void BuildMethod(Type targetType, MethodType methodType, Delegate method = null)
		{
			if (methodType == MethodType.ReadOne)
			{
				FieldInfo fi = GetReadOneCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = ReadOneBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else if (methodType == MethodType.ReadFix)
			{
				FieldInfo fi = GetReadFixCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = ReadFixBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else if (methodType == MethodType.ReadVar)
			{
				FieldInfo fi = GetReadVarCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = ReadVarBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else if (methodType == MethodType.WriteOne)
			{
				FieldInfo fi = GetWriteOneCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = WriteOneBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else if (methodType == MethodType.WriteFix)
			{
				FieldInfo fi = GetWriteFixCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = WriteFixBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else if (methodType == MethodType.WriteVar)
			{
				FieldInfo fi = GetWriteVarCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = WriteVarBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else
				throw new NotImplementedException("unknown method type");
		}

		protected void AppendBuildRequest(Type targetType, MethodType methodType)
		{
			lock (_dependencySync)
				_dependency.Enqueue(new BuildRequest { TargetType = targetType, Method = methodType });
		}

		public IReader CreateReader(IByteReader reader)
		{
			return new Reader(this, reader);
		}

		public IWriter CreateWriter(IByteWriter writer)
		{
			return new Writer(this, writer);
		}
	}
}
