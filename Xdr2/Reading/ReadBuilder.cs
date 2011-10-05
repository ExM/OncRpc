using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
using Xdr2.Emit;

namespace Xdr2
{
	public sealed partial class ReadBuilder
	{
		private ReadMapper _rm;
		private Type _dynReaderType;

		private ModuleBuilder _modBuilder;
		private BuildBinderDescription _buildBinderDescription;
		private StaticCacheDescription _oneCacheDescription;
		private StaticCacheDescription _varCacheDescription;
		private StaticCacheDescription _fixCacheDescription;
		
		public ReadBuilder()
		{
			
			string name = "DynamicXdrReadMapper";
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");

			_buildBinderDescription = new BuildBinderDescription(_modBuilder);
			_oneCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "OneCache", true, OpaqueType.One);
			_fixCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "FixCache", true, OpaqueType.Fix);
			_varCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "VarCache", true, OpaqueType.Var);


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

