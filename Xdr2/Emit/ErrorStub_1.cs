using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr2
{
	internal sealed class ErrorStub<T>
	{
		public readonly Exception Error;

		public ErrorStub(Exception ex)
		{
			Error = ex;
		}

		public T ReadOne(Reader reader)
		{
			throw Error;
		}
		
		public T ReadMany(Reader reader, uint len)
		{
			throw Error;
		}
		
		public void WriteOne(Writer writer, T v)
		{
			throw Error;
		}
		
		public void WriteMany(Writer writer, uint len, T v)
		{
			throw Error;
		}
	}
}
