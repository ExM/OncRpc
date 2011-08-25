using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Translating;

namespace Xdr
{
	public sealed class TranslatorBuilder
	{
		private Xdr.Examples.Translator _t;
		
		internal TranslatorBuilder(string name)
		{
			_t = new Xdr.Examples.Translator();
		}

		public TranslatorBuilder Map<T>(ReadOneDelegate<T> reader)
		{
			_t.AppendMethod(typeof(T), MethodType.ReadOne, reader);
			return this;
		}

		public TranslatorBuilder Map<T>(ReadManyDelegate<T> reader)
		{
			_t.AppendMethod(typeof(T), MethodType.ReadMany, reader);
			return this;
		}

		public ITranslator Build()
		{
			return _t;
		}
	}
}

