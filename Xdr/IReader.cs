using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface IReader
	{
		void Read<T>(Action<T> completed, Action<Exception> excepted);
		void Read<T>(uint len, bool fix, Action<T> completed, Action<Exception> excepted);
	}
}
