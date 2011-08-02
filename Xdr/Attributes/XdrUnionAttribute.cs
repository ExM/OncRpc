using System;

namespace Xdr
{
	public class XdrUnionAttribute: Attribute
	{
		public uint Order {get; set;}
		
		public XdrUnionAttribute ()
		{
		}
	}
}

