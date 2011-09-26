using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xdr.EmitContexts
{
	public static class EmitContext
	{
		private static ModuleBuilder _modBuilder;
		private static Dictionary<Type, EmitResult> _readerCache = new Dictionary<Type, EmitResult>();
		private static Dictionary<Type, EmitResult> _writerCache = new Dictionary<Type, EmitResult>();
		
		static EmitContext()
		{
			string name = "DynamicXdrEmitContexts";
			AssemblyName asmName = new AssemblyName(name);
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = asmBuilder.DefineDynamicModule(name + ".dll", name + ".dll");
		}
		
		public static Delegate GetReader(Type targetType)
		{
			lock(_readerCache)
			{
				EmitResult result;
				if(!_readerCache.TryGetValue(targetType, out result))
				{
					result = new EmitResult();
					try
					{
						result.Method = EmitReader(_modBuilder, targetType);
					}
					catch(Exception ex)
					{
						result.Error = new InvalidOperationException("can't emit reader", ex);
					}
					_readerCache.Add(targetType, result);
				}
				
				if(result.Error != null)
					throw result.Error;
				return result.Method;
			}
		}
		
		public static Delegate GetWriter(Type targetType)
		{
			lock(_writerCache)
			{
				EmitResult result;
				if(!_writerCache.TryGetValue(targetType, out result))
				{
					result = new EmitResult();
					try
					{
						result.Method = EmitWriter(_modBuilder, targetType);
					}
					catch(Exception ex)
					{
						result.Error = new InvalidOperationException("can't emit writer", ex);
					}
					_writerCache.Add(targetType, result);
				}
				
				if(result.Error != null)
					throw result.Error;
				return result.Method;
			}
		}
		
		public static Delegate EmitReader(ModuleBuilder mb, Type targetType)
		{
			List<FieldDesc> fields = OrderModel.GetFields(targetType);
			SwitchModel swModel = SwitchModel.Create(targetType);
			
			Type contextType;
			
			if(fields == null)
			{
				if(swModel == null)
					return null;
				contextType = swModel.BuildReadContext(mb, targetType);
			}
			else
			{
				if(swModel != null)
					throw new InvalidOperationException("unknown way to convert");
				contextType = OrderModel.BuildReadContext(mb, targetType, fields);
			}
			
			MethodInfo mi = contextType.GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}
		
		public static Delegate EmitWriter(ModuleBuilder mb, Type targetType)
		{
			List<FieldDesc> fields = OrderModel.GetFields(targetType);
			if(fields == null)
				return null;
			
			Type contextType = OrderModel.BuildWriteContext(mb, targetType, fields);
			MethodInfo mi = contextType.GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}
	}
}

