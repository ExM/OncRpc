using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.IO;
using Xdr.Emit;

namespace Xdr
{
	public sealed partial class WriteBuilder
	{
		private WriteMapper _wm;
		private Type _dynWriterType;

		private ModuleBuilder _modBuilder;
		private BuildBinderDescription _buildBinderDescription;
		private StaticCacheDescription _oneCacheDescription;
		private StaticCacheDescription _varCacheDescription;
		private StaticCacheDescription _fixCacheDescription;
		
		public WriteBuilder()
		{
			
			string name = "DynamicXdrWriteMapper";
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");

			_buildBinderDescription = new BuildBinderDescription(_modBuilder);
			_oneCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "OneCache", false, OpaqueType.One);
			_fixCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "FixCache", false, OpaqueType.Fix);
			_varCacheDescription = new StaticCacheDescription(_modBuilder, _buildBinderDescription, "VarCache", false, OpaqueType.Var);


			Type dynWriteMapperType = EmitDynWriteMapper();

			_wm = (WriteMapper)Activator.CreateInstance(dynWriteMapperType);

			_dynWriterType = EmitDynWriter();

			FieldInfo mapperInstance = _dynWriterType.GetField("Mapper", BindingFlags.Public | BindingFlags.Static);
			mapperInstance.SetValue(null, _wm);
		}

		public WriteBuilder Map<T>(WriteOneDelegate<T> writer)
		{
			_wm.AppendMethod(typeof(T), OpaqueType.One, writer);
			return this;
		}

		public WriteBuilder MapFix<T>(WriteManyDelegate<T> writer)
		{
			_wm.AppendMethod(typeof(T), OpaqueType.Fix, writer);
			return this;
		}
		
		public WriteBuilder MapVar<T>(WriteManyDelegate<T> writer)
		{
			_wm.AppendMethod(typeof(T), OpaqueType.Var, writer);
			return this;
		}

		public Writer Create(IByteWriter writer)
		{
			return (Writer)Activator.CreateInstance(_dynWriterType, writer); // HACK: emit creater
		}
	}
}

