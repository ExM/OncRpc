using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr2
{
	internal sealed class BuildRequest
	{
		public Type TargetType { get; set; }
		public OpaqueType Method { get; set; }
	}
}
