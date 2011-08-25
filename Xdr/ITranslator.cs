using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface ITranslator
	{
		IReader Create(IByteReader reader);
		//IWriter Create(IByteWriter reader);
	}
}
