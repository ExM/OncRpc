using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public abstract class Reader
	{
		public readonly IByteReader ByteReader;

		protected Reader(IByteReader reader)
		{
			ByteReader = reader;
		}

		public abstract T Read<T>();
		
		public abstract T ReadFix<T>(uint len);
		
		public abstract T ReadVar<T>(uint max);
		
		public T ReadOption<T>() where T: class
		{
			if (Read<bool>())
				return Read<T>();
			else
				return null;
		}
	}
}
