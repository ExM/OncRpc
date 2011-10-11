using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// rpcbind procedures
	/// http://tools.ietf.org/html/rfc1833#section-2.1
	/// </summary>
	public partial class Bind : DefineProgram
	{
		public override uint Program
		{
			get { return 100000u; }
		}

		public readonly Version3 V3 = new Version3();
		public readonly Version4 V4 = new Version4();
	}
}
