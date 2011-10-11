using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class ReaderExample : Reader
	{
		public static ReadMapper MapperInstance;
		
		public ReaderExample(IByteReader r): base(r)
		{
		}

		protected override T CacheRead<T>()
		{
			if (OneCache<T>.Instance == null)
				MapperInstance.BuildCaches();
			return OneCache<T>.Instance(this);
		}

		protected override T CacheReadFix<T>(uint len)
		{
			throw new NotImplementedException();
		}

		protected override T CacheReadVar<T>(uint max)
		{
			throw new NotImplementedException();
		}
	}
}
