using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Translating;
using Xdr.Translating.Emit;

namespace Xdr
{
	public sealed partial class TranslatorBuilder
	{
		private BaseTranslator _t;

		private ModuleBuilder _modBuilder;
		private DelegateCacheDescription _delegateCacheDescription;
		private ReadOneCacheDescription _readOneCacheDescription;
		private ReadManyCacheDescription _readManyCacheDescription;
		
		internal TranslatorBuilder(string name)
		{
			//_t = new Xdr.Examples.Translator();

			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");

			_delegateCacheDescription = new DelegateCacheDescription(_modBuilder);
			_readOneCacheDescription = new ReadOneCacheDescription(_modBuilder, _delegateCacheDescription);
			_readManyCacheDescription = new ReadManyCacheDescription(_modBuilder, _delegateCacheDescription);
			
			Type dynTrType = EmitDynTranslator();
			
			_t = (BaseTranslator)Activator.CreateInstance(dynTrType);
			
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

