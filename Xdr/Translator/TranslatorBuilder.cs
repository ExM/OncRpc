using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr
{
	public class TranslatorBuilder
	{
		private ModuleBuilder _modBuilder;
		private Func<Type, Delegate> _readOneContextMap;

		internal TranslatorBuilder(string name)
		{

		}

		public TranslatorBuilder Map<T>(ReadOneDelegate<T> reader)
		{
			
			return this;
		}

		public ITranslator Build()
		{
			return null;
		}
	}
}

