using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Translating
{
	internal sealed class BuildRequest
	{
		public Type TargetType { get; set; }
		public MethodType Method { get; set; }
	}
}
