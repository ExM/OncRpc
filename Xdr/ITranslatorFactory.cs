using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface ITranslatorFactory
	{
		IReader Create(IByteReader reader);
		IWriter Create(IByteWriter writer);
	}
}
