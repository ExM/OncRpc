using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc
{
	public class DefineProcedure<TReq, TResp>
	{
		public readonly uint Proc;

		public DefineProcedure(uint proc)
		{
			Proc = proc;
		}
	}
}
