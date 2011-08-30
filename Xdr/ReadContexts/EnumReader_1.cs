using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	public class EnumReader<T>
		where T: struct
	{
		private static readonly Dictionary<int, T> _enumMap;

		private Action<T> _completed;
		private Action<Exception> _excepted;

		static EnumReader()
		{
			Type underType = typeof(T).GetEnumUnderlyingType();
			Func<object, int> conv;
			if(underType == typeof(byte))
				conv = (item) => (int)(byte)(ValueType)item;
			else if(underType == typeof(sbyte))
				conv = (item) => (int)(sbyte)(ValueType)item;
			else if(underType == typeof(short))
				conv = (item) => (int)(short)(ValueType)item;
			else if(underType == typeof(ushort))
				conv = (item) => (int)(ushort)(ValueType)item;
			else if(underType == typeof(int))
				conv = (item) => (int)(ValueType)item;
			else
				throw new NotSupportedException(string.Format("unsupported type {0}", typeof(T).FullName));
			
			_enumMap = new Dictionary<int,T>();
			foreach (object item in Enum.GetValues(typeof(T)))
			{
				T exist;
				int key = conv(item);
				if(!_enumMap.TryGetValue(key, out exist))
					_enumMap.Add(key, (T)item);
			}
		}

		public EnumReader(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			_completed = completed;
			_excepted = excepted;
			reader.ReadInt32(target_Readed, _excepted);
		}

		private void target_Readed(int val)
		{
			T exist;
			if (_enumMap.TryGetValue(val, out exist))
				_completed(exist);
			else
				_excepted(new InvalidCastException(string.Format("type `{0}' not contain {1}", typeof(T).FullName, val)));
		}

		public static void Read(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			new EnumReader<T>(reader, completed, excepted);
		}
	}
}
