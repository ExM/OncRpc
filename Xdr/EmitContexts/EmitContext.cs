using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Xdr.EmitContexts
{
	public static class EmitContext
	{
		public const string DynAssemblyName = "Xdr.EmitContexts.Dynamic";

		private static object _sync = new object();
		private static AssemblyBuilder _asmBuilder;
		private static ModuleBuilder _modBuilder;
		private static Dictionary<Type, EmitResult> _readerCache;
		private static Dictionary<Type, EmitResult> _writerCache;
		
		static EmitContext()
		{
			Init();
		}

		private static void Init()
		{
			_readerCache = new Dictionary<Type, EmitResult>();
			_writerCache = new Dictionary<Type, EmitResult>();

			AssemblyName asmName = new AssemblyName(DynAssemblyName);
			_asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			_modBuilder = _asmBuilder.DefineDynamicModule(DynAssemblyName + ".dll", DynAssemblyName + ".dll");
		}

		public static void SaveDynamicAssembly()
		{
			lock (_sync)
			{
				_asmBuilder.Save(DynAssemblyName + ".dll");
				Init();
			}
		}
		
		public static Delegate GetReader(Type targetType)
		{
			lock (_sync)
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
			lock (_sync)
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
			OrderModel ordModel = OrderModel.Create(targetType);
			SwitchModel swModel = SwitchModel.Create(targetType);
			
			Type contextType;

			if (ordModel == null)
			{
				if(swModel == null)
					return null;
				contextType = swModel.BuildReadContext(mb, targetType);
			}
			else
			{
				if(swModel != null)
					throw new InvalidOperationException("unknown way to convert");
				contextType = ordModel.BuildReadContext(mb, targetType);
			}
			
			MethodInfo mi = contextType.GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}
		
		public static Delegate EmitWriter(ModuleBuilder mb, Type targetType)
		{
			OrderModel ordModel = OrderModel.Create(targetType);
			SwitchModel swModel = SwitchModel.Create(targetType);

			Type contextType;

			if (ordModel == null)
			{
				if (swModel == null)
					return null;
				contextType = swModel.BuildWriteContext(mb, targetType);
			}
			else
			{
				if (swModel != null)
					throw new InvalidOperationException("unknown way to convert");
				contextType = ordModel.BuildWriteContext(mb, targetType);
			}
			
			MethodInfo mi = contextType.GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}
	}
}

