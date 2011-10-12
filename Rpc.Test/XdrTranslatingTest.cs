using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Rpc.MessageProtocol;
using Xdr;
using Rpc.BindingProtocols;

namespace Rpc
{
	[TestFixture]
	public class XdrTranslatingTest
	{
		[Test]
		public void PortMapperDump()
		{
			
/* TODO: check Xdr-Rpc read/write
sending byte dump:
F1E2D3C4 00000000 00000002 000186A0 00000002 00000004 00000000 00000000
00000000 00000000
received byte dump: 
F1E2D3C4 00000001 00000000 00000000 00000000 00000000 00000001 000186A0
00000002 00000006 0000006F 00000001 000186A0 00000002 00000011 0000006F
00000001 000186B8 00000001 00000011 0000BFA1 00000001 000186B8 00000001
00000006 0000E10D 00000000

port:111 prog:100000 prot:6 vers:2
port:111 prog:100000 prot:17 vers:2
port:49057 prog:100024 prot:17 vers:1
port:57613 prog:100024 prot:6 vers:1
*/

		}

		[Test]
		public void PortMapperNull()
		{
			/*
	sending byte dump: 
	F1E2D3C4 00000000 00000002 000186A0 00000002 00000000 00000000 00000000
	00000000 00000000
	received byte dump: 
	F1E2D3C4 00000001 00000000 00000000 00000000 00000000
	*/
		}
	}
}

