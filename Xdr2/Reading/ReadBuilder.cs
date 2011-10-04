using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Translating;
using System.IO;
using Xdr.Translating.Emit;

namespace Xdr2
{
	public sealed partial class ReadBuilder
	{
		private ReadMapper _rm;
		private Type _dynReaderType;
		
		public ReadBuilder()
		{
			/*
			string name = "DynamicXdrTranslator";
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");

			_delegateCacheDescription = new DelegateCacheDescription(_modBuilder);
			_readOneCacheDescription = new ReadOneCacheDescription(_modBuilder, _delegateCacheDescription);
			_readFixCacheDescription = new ReadFixCacheDescription(_modBuilder, _delegateCacheDescription);
			_readVarCacheDescription = new ReadVarCacheDescription(_modBuilder, _delegateCacheDescription);
			_writeOneCacheDescription = new WriteOneCacheDescription(_modBuilder, _delegateCacheDescription);
			_writeFixCacheDescription = new WriteFixCacheDescription(_modBuilder, _delegateCacheDescription);
			_writeVarCacheDescription = new WriteVarCacheDescription(_modBuilder, _delegateCacheDescription);

			Type dynTrType = EmitDynTranslator();

			_t = (BaseTranslator)Activator.CreateInstance(dynTrType);
			*/
		}

		public ReadBuilder Map<T>(ReadOneDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), SplitType.One, reader);
			return this;
		}

		public ReadBuilder MapFix<T>(ReadManyDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), SplitType.Fix, reader);
			return this;
		}
		
		public ReadBuilder MapVar<T>(ReadManyDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), SplitType.Var, reader);
			return this;
		}

		public Reader Create(IByteReader reader)
		{
			return (Reader)Activator.CreateInstance(_dynReaderType, reader);
		}
	}
}

