using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Xdr.Emit
{
	public class XdrFieldDesc
	{
		private FieldInfo _fi;
		public Type Type {get; private set;}
		
		
		public XdrFieldDesc(FieldInfo fi)
		{
			_fi = fi;
			Type = _fi.FieldType;
		}
	}
}

