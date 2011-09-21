using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Translating
{
	public enum MethodType
	{
		ReadOne = 0,
		ReadFix = 1,
		ReadVar = 2,
		WriteOne = 3,
		WriteFix = 4,
		WriteVar = 5
	}
}
