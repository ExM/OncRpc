using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Xdr.Translating;

namespace Xdr.Examples
{
	public static class DelegateCache
	{
		public static Action<Type, MethodType> BuildRequest = null;
	}
}
