using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface ITranslator
	{
		Reader CreateReader(IByteReader reader);
		Writer CreateWriter(IByteWriter reader);
	}
}
