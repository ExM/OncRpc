using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr2;

namespace EmitTest
{
	public class ReaderExample : Reader
	{
		public static ReadMapper MapperInstance;
		
		public ReaderExample(IByteReader r): base(r)
		{
		}
		
		public override T Read<T> ()
		{
			if(OneCache<T>.Instance == null)
				MapperInstance.BuildCaches();
			return OneCache<T>.Instance(this);
		}

		public override T ReadFix<T> (uint len)
		{
			throw new NotImplementedException ();
		}

		public override T ReadVar<T> (uint max)
		{
			throw new NotImplementedException ();
		}
	}
}
