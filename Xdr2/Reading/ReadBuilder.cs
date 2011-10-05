using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
using Xdr2.Reading.Emit;

namespace Xdr2
{
	public sealed partial class ReadBuilder
	{
		private ReadMapper _rm;
		private Type _dynReaderType;

		private ModuleBuilder _modBuilder;
		private DelegateCacheDescription _delegateCacheDescription;
		private OneCacheDescription _oneCacheDescription;
		private VarCacheDescription _varCacheDescription;
		private FixCacheDescription _fixCacheDescription;
		
		public ReadBuilder()
		{
			
			string name = "DynamicXdrReadMapper";
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");

			_delegateCacheDescription = new DelegateCacheDescription(_modBuilder);
			_oneCacheDescription = new OneCacheDescription(_modBuilder, _delegateCacheDescription);
			_fixCacheDescription = new FixCacheDescription(_modBuilder, _delegateCacheDescription);
			_varCacheDescription = new VarCacheDescription(_modBuilder, _delegateCacheDescription);


			Type dynReadMapperType = EmitDynReadMapper();

			_rm = (ReadMapper)Activator.CreateInstance(dynReadMapperType);

			_dynReaderType = EmitDynReader();

			FieldInfo mapperInstance = _dynReaderType.GetField("Mapper", BindingFlags.Public | BindingFlags.Static);
			mapperInstance.SetValue(null, _rm);
		}

		public ReadBuilder Map<T>(ReadOneDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), OpaqueType.One, reader);
			return this;
		}

		public ReadBuilder MapFix<T>(ReadManyDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), OpaqueType.Fix, reader);
			return this;
		}
		
		public ReadBuilder MapVar<T>(ReadManyDelegate<T> reader)
		{
			_rm.AppendMethod(typeof(T), OpaqueType.Var, reader);
			return this;
		}

		public Reader Create(IByteReader reader)
		{
			return (Reader)Activator.CreateInstance(_dynReaderType, reader); // HACK: emit creater
		}
	}
}

