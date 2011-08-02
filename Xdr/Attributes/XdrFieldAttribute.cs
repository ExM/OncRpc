using System;

namespace Xdr
{
	public class XdrFieldAttribute: Attribute
	{
		public uint Order {get; set;}
		public uint MaxLength {get; set;}
		
		public XdrFieldAttribute ()
		{
		}
	}
}

