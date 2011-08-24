using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr.Examples
{
	public class Translator: ITranslator
	{
		private object _sync = new object();

		private object _dependencySync = new object();
		private Queue<BuildRequest> _dependency = new Queue<BuildRequest>();

		public Translator()
		{
			DelegateCache.BuildRequest = BuildRequest;
		}
		
		public void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadOneCache<T>.Instance == null)
				BuildCaches();
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}

		public void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadManyCache<T>.Instance == null)
				BuildCaches();
			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}

		private void BuildCaches()
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

					if (bReq.Method == MethodType.ReadOne)
					{
						Delegate reader = ReadOneBuild(bReq.TargetType);
						typeof(ReadOneCache<>).MakeGenericType(bReq.TargetType).GetField("Instance").SetValue(null, reader);
					}
					else if (bReq.Method == MethodType.ReadMany)
					{


					}
					else
						throw new NotImplementedException("unknown method type");
				}
			}
		}



		private Delegate ReadOneBuild(Type targetType)
		{
			if(targetType == typeof(Int32))
				return (Delegate)(ReadOneDelegate<Int32>)ReadInt32;

			try
			{



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

			try
			{



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

		private void BuildRequest(Type targetType, MethodType methodType)
		{
			lock (_dependencySync)
				_dependency.Enqueue(new BuildRequest { TargetType = targetType, Method = methodType });
		}
		
		public void ReadInt32(IReader reader, Action<int> completed, Action<Exception> excepted)
		{
			reader.ReadInt32(completed, excepted);
		}
		
		public void ReadBytes(IReader reader, uint len, bool fix, Action<byte[]> completed, Action<Exception> excepted)
		{
			if(fix)
				reader.ReadFixOpaque(len, completed, excepted);
			else
				reader.ReadVarOpaque(len, completed, excepted);
		}

		public IReader Create(IByteReader reader)
		{
			return new Reader(this, reader);
		}
	}
}
