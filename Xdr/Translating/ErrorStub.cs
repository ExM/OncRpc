using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Translating
{
	internal sealed class ErrorStub<T>
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
		
		public void ReadFix(IReader reader, uint len, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}

		public void ReadVar(IReader reader, uint max, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}
		
		public void Write(IWriter writer, T item, Action completed, Action<Exception> excepted)
		{
			writer.Throw(Error, excepted);
		}
	}
}
