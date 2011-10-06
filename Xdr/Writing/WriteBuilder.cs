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
		private Func<IByteWriter, Writer> _creater;

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

			Type dynWriterType = EmitDynWriter();

			FieldInfo mapperInstance = dynWriterType.GetField("Mapper", BindingFlags.Public | BindingFlags.Static);
			mapperInstance.SetValue(null, _wm);

			_creater = EmitCreater(dynWriterType.GetConstructor(new Type[] { typeof(IByteWriter) }));
		}

		private static Func<IByteWriter, Writer> EmitCreater(ConstructorInfo ci)
		{
			var dm = new DynamicMethod("DynCreateWriter", typeof(Writer), new Type[] { typeof(IByteWriter) }, typeof(WriteBuilder), true);
			var il = dm.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, ci);
			il.Emit(OpCodes.Ret);
			return (Func<IByteWriter, Writer>)dm.CreateDelegate(typeof(Func<IByteWriter, Writer>));
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
			return _creater(writer);
		}
	}
}

