using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface IWriter
	{
		void Write<T>(T obj, Action completed, Action<Exception> excepted);
		void Write<T>(T list, uint len, bool fix, Action completed, Action<Exception> excepted);
	}
}
