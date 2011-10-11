using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rpc
{
	public class ProcedureDescription<TReq, TResp>
	{
		public uint Program {get; private set;}
		public uint Version {get; private set;}
		public uint Procedure {get; private set;}

		public ProcedureDescription(uint prog, uint ver, uint proc)
		{
			Program = prog;
			Version = ver;
			Procedure = proc;
		}
	}
}
