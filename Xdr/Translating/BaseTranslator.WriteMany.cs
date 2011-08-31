using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.ReadContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
	{
		private Delegate WriteManyBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				//TODO: write many build

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "WriteMany", targetType, typeof(WriteManyDelegate<>));
			}
		}
	}
}
