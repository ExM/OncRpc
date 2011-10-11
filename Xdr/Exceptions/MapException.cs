using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public class MapException: SystemException
	{
		public MapException()
			: base()
		{
		}

		public MapException(string message)
			: base(message)
		{
		}

		public MapException(string message, Exception innerEx)
			: base(message, innerEx)
		{
		}

		public static MapException ReadOne(Type type, Exception innerEx)
		{
			return new MapException(string.Format("can't read an instance of `{0}'", type.FullName), innerEx);
		}

		internal static Exception ReadVar(Type type, uint max, SystemException innerEx)
		{
			return new MapException(string.Format("can't read collection of `{0}' (length <= {1})", type.FullName, max), innerEx);
		}

		internal static Exception ReadFix(Type type, uint len, SystemException innerEx)
		{
			return new MapException(string.Format("can't read collection of `{0}' (length = {1})", type.FullName, len), innerEx);
		}

		internal static Exception WriteOne(Type type, SystemException innerEx)
		{
			return new MapException(string.Format("can't write an instance of `{0}'", type.FullName), innerEx);
		}

		internal static Exception WriteFix(Type type, uint len, SystemException innerEx)
		{
			return new MapException(string.Format("can't write collection of `{0}' (length = {1})", type.FullName, len), innerEx);
		}

		internal static Exception WriteVar(Type type, uint max, SystemException innerEx)
		{
			return new MapException(string.Format("can't write collection of `{0}' (length <= {1})", type.FullName, max), innerEx);
		}
	}
}
