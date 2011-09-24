using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.WriteContexts
{
	public class EnumWriter<T>
		where T: struct
	{
		private static readonly Dictionary<T, int> _enumMap;

		static EnumWriter()
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
			
			_enumMap = new Dictionary<T, int>();
			foreach (object item in Enum.GetValues(typeof(T)))
				_enumMap.Add((T)item, conv(item));
		}

		public static void Write(Writer writer, T item, Action completed, Action<Exception> excepted)
		{
			int val;
			if (_enumMap.TryGetValue(item, out val))
				writer.WriteInt32(val, completed, excepted);
			else
				writer.Throw(new InvalidCastException(string.Format("enum {0} not contain value {1}", typeof(T).FullName, item)), excepted);
		}
	}
}
