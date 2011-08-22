using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Examples
{
	public class Translator: ITranslator
	{
		private object _sync = new object();

		private Queue<Type> _dependency = new Queue<Type>();
		private Dictionary<Type, List<Type>> _errorParents = new Dictionary<Type, List<Type>>();

		public void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadOneCache<T>.Instance == null)
			{
				lock (_sync)
				{
					if(ReadOneCache<T>.Instance == null)
					{




					}
				}
			}
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}


		public void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{


			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}
	}
}
