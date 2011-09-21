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
		
		public void ReadMany(IReader reader, uint len, Action<T> completed, Action<Exception> excepted)
		{
			reader.Throw(Error, excepted);
		}
		
		public void WriteOne(IWriter writer, T item, Action completed, Action<Exception> excepted)
		{
			writer.Throw(Error, excepted);
		}
		
		public void WriteMany(IWriter writer, T items, uint len, Action completed, Action<Exception> excepted)
		{
			writer.Throw(Error, excepted);
		}
	}
}
