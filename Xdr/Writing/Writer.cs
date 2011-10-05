using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public abstract class Writer
	{
		public readonly IByteWriter ByteWriter;

		protected Writer(IByteWriter writer)
		{
			ByteWriter = writer;
		}

		public abstract void Write<T>(T item);
		
		public abstract void WriteFix<T>(uint len, T item);
		
		public abstract void WriteVar<T>(uint max, T item);
		
		public void WriteOption<T>(T item) where T: class
		{
			if(item == null)
				Write<bool>(false);
			else
			{
				Write<bool>(true);
				Write<T>(item);
			}
		}
	}
}
