using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.ReadContexts
{
	/// <summary>
	/// Bool reader.
	/// http://tools.ietf.org/html/rfc4506#section-4.4
	/// </summary>
	public class BoolReader
	{
		private Action<bool> _completed;
		private Action<Exception> _excepted;

		public BoolReader(IReader reader, Action<bool> completed, Action<Exception> excepted)
		{
			_completed = completed;
			_excepted = excepted;
			reader.ReadInt32(target_Readed, _excepted);
		}

		private void target_Readed(int val)
		{
			if (val == 0)
				_completed(false);
			else if(val == 1)
				_completed(true);
			else
				_excepted(new InvalidCastException(string.Format("no boolean value `{0}'", val)));
		}

		public static void Read(IReader reader, Action<bool> completed, Action<Exception> excepted)
		{
			new BoolReader(reader, completed, excepted);
		}
	}
}
