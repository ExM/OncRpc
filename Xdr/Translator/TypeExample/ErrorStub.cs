using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Translating
{
	public class ErrorStub<T>
	{
		public readonly Exception Error;

		public ErrorStub(Exception ex)
		{
			Error = ex;
		}

		public void ReadOne(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}

		public void ReadMany(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}
	}
}
