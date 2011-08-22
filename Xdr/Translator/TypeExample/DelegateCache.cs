using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class DelegateCache
	{
		public static Action<Type> ReadOneInit;
		public static Action<Type> ReadManyInit;
		public static Action<Type> WriteOneInit;
		public static Action<Type> WriteManyInit;
	}
}
