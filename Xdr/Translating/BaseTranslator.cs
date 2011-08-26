using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;

namespace Xdr
{
	public abstract class BaseTranslator: ITranslator
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();
		
		protected BaseTranslator()
		{
		}

		protected abstract Type GetReadOneCacheType();
		public abstract void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted);

		protected abstract Type GetReadManyCacheType();
		public abstract void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted);

		protected abstract Type GetWriteOneCacheType();
		public abstract void Write<T>(IWriter writer, T item, Action completed, Action<Exception> excepted);

		protected abstract Type GetWriteManyCacheType();
		public abstract void Write<T>(IWriter writer, T items, bool fix, Action completed, Action<Exception> excepted);
		
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
			else if (methodType == MethodType.ReadMany)
			{
				FieldInfo fi = GetReadManyCacheType().MakeGenericType(targetType).GetField("Instance");
				if(fi.GetValue(null) == null)
				{
					if(method == null)
						method = ReadManyBuild(targetType);
					fi.SetValue(null, method);
				}
			}
			else
				throw new NotImplementedException("unknown method type");
		}

		private Delegate ReadOneBuild(Type targetType)
		{
			if(targetType == typeof(Int32))
				return (Delegate)(ReadOneDelegate<Int32>)ReadInt32;
			if(targetType == typeof(UInt32))
				return (Delegate)(ReadOneDelegate<UInt32>)ReadUInt32;

			try
			{
				ReadOneAttribute attr = targetType.GetCustomAttributes(typeof(ReadOneAttribute), true)
					.Select((o) => (ReadOneAttribute)o)
					.FirstOrDefault();
				if (attr != null)
					return attr.Create(targetType);







				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadOne", targetType, typeof(ReadOneDelegate<>));
			}
		}

		private Delegate ReadManyBuild(Type targetType)
		{
			if(targetType == typeof(byte[]))
				return (Delegate)(ReadManyDelegate<byte[]>)ReadBytes;
			if(targetType == typeof(string))
				return (Delegate)(ReadManyDelegate<string>)ReadString;
			
			try
			{
				Type itemType = targetType.GetKnownItemType();
				if (itemType != null)
				{
					Delegate attrResult = null;
					foreach (ReadManyAttribute attr in itemType.GetCustomAttributes(typeof(ReadManyAttribute), true)
						.Select((o) => (ReadManyAttribute)o))
					{
						Delegate result = attr.Create(targetType);
						if (attrResult != null)
							throw new InvalidOperationException(string.Format("Duplicate methods in {0} ({1}.{2} & {2}.{3})",
								targetType, result.Method.DeclaringType, result.Method.Name, attrResult.Method.DeclaringType, attrResult.Method.Name));
						attrResult = result;
					}

					if (attrResult != null)
						return attrResult;
				}






				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadMany", targetType, typeof(ReadManyDelegate<>));
			}
		}

		private Delegate CreateStubDelegate(Exception ex, string method, Type targetType, Type genDelegateType)
		{
			Type stubType = typeof(ErrorStub<>).MakeGenericType(targetType);
			object stubInstance = Activator.CreateInstance(stubType, ex);
			MethodInfo mi = stubType.GetMethod(method);
			return Delegate.CreateDelegate(genDelegateType.MakeGenericType(targetType), stubInstance, mi);
		}

		protected void AppendBuildRequest(Type targetType, MethodType methodType)
		{
			lock (_dependencySync)
				_dependency.Enqueue(new BuildRequest { TargetType = targetType, Method = methodType });
		}
		
		private static void ReadInt32(IReader reader, Action<int> completed, Action<Exception> excepted)
		{
			reader.ReadInt32(completed, excepted);
		}
		
		private static void ReadUInt32(IReader reader, Action<uint> completed, Action<Exception> excepted)
		{
			reader.ReadUInt32(completed, excepted);
		}
		
		private static void ReadBytes(IReader reader, uint len, bool fix, Action<byte[]> completed, Action<Exception> excepted)
		{
			if(fix)
				reader.ReadFixOpaque(len, completed, excepted);
			else
				reader.ReadVarOpaque(len, completed, excepted);
		}
		
		private static void ReadString(IReader reader, uint len, bool fix, Action<string> completed, Action<Exception> excepted)
		{
			reader.ReadString(len, completed, excepted);
		}

		public IReader CreateReader(IByteReader reader)
		{
			return new Reader(this, reader);
		}

		public IWriter CreateWriter(IByteWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
