using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Examples
{
	public class Translator: ITranslator
	{
		private object _sync = new object();

		//private Queue<Type> _dependency = new Queue<Type>();
		//private Dictionary<Type, List<Type>> _errorParents = new Dictionary<Type, List<Type>>();


		public Translator()
		{
			DelegateCache.ReadOneBuild = ReadOneBuild;
			DelegateCache.ReadManyBuild = ReadManyBuild;
		}
		
		public void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}

		public void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}
		
		private Delegate ReadOneBuild(Type targetType)
		{
			if(targetType == typeof(Int32))
				return (Delegate)(ReadOneDelegate<Int32>)ReadInt32;
			
			
			throw new NotImplementedException();
		}
		
		private Delegate ReadManyBuild(Type targetType)
		{
			if(targetType == typeof(byte[]))
				return (Delegate)(ReadManyDelegate<byte[]>)ReadBytes;
			
			
			throw new NotImplementedException();
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
	}
}
