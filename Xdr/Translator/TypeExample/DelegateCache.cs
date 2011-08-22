using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace Xdr.Examples
{
	public static class DelegateCache
	{
		public static Func<Type, Delegate> ReadOneBuild;
		public static Func<Type, Delegate> ReadManyBuild;
		/*
		public static Action<Type> WriteOneInit;
		public static Action<Type> WriteManyInit;
		*/
	}
}
