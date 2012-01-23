using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class ReadMapperExample: ReadMapper
	{
		private Type _oneCacheType;
		private Type _fixCacheType;
		private Type _varCacheType;
		
		public ReadMapperExample()
		{
			_oneCacheType = null;
			_fixCacheType = null;
			_varCacheType = null;
		}

		protected override Type GetOneCacheType ()
		{
			return _oneCacheType;
		}

		protected override Type GetFixCacheType ()
		{
			return _fixCacheType;
		}

		protected override Type GetVarCacheType ()
		{
			return _varCacheType;
		}
	}
}
