using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public class EnumHelper<T>
		where T: struct
	{
		private static readonly Dictionary<T, int> _enumMap;
		private static readonly Dictionary<int, T> _intMap;

		static EnumHelper()
		{
			Type underType = typeof(T).GetEnumUnderlyingType();
			Func<T, int> conv;
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
			
			_intMap = new Dictionary<int,T>();
			_enumMap = new Dictionary<T, int>();
			
			foreach (T item in Enum.GetValues(typeof(T)).Cast<T>())
			{
				T exist;
				int key = conv(item);
				if(!_intMap.TryGetValue(key, out exist))
					_intMap.Add(key, item);
				
				
				if(!_enumMap.TryGetValue(item, out key))
					_enumMap.Add(item, conv(item));
			}
		}

		public static void IntToEnum(int val, Action<T> completed, Action<Exception> excepted)
		{
			T exist;
			if (_intMap.TryGetValue(val, out exist))
				completed(exist);
			else
				excepted(new InvalidCastException(string.Format("type `{0}' not contain {1}", typeof(T).FullName, val)));
		}
		
		public static void EnumToInt(T item, Action<int> completed, Action<Exception> excepted)
		{
			int val;
			if (_enumMap.TryGetValue(item, out val))
				completed(val);
			else
				excepted(new InvalidCastException(string.Format("enum {0} not contain value {1}", typeof(T).FullName, item)));
		}
	}
}