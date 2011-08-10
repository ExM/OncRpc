using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Xdr
{
	public static class XdrReadBuilder
	{
		private static ModuleBuilder _modBuilder;
		private static object _sync = new object();
		
		static XdrReadBuilder()
		{
			AssemblyName asmName = new AssemblyName("DynamicAssembly_XdrReading");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
			_modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);
		}
		
		public static ReaderDelegate<T> Build<T>()
		{
			if(TypeCache<T>.ReadContextExcepted != null)
				throw TypeCache<T>.ReadContextExcepted;
			
			if(TypeCache<T>.ReadContextType != null)
				return TypeCache<T>.Read;
			
			lock(_sync)
			{
				if(TypeCache<T>.ReadContextType == null)
				{
					List<Type> dependency = new List<Type>();
					try
					{
						TypeCache<T>.ReadContextType = CreateContextType(typeof(T), dependency);
					}
					catch(Exception ex)
					{
						TypeCache<T>.ReadContextExcepted = ex;
						throw;
					}
					
					try
					{
						while(dependency.Count != 0)
						{
							Type depType = dependency[0];
							dependency.RemoveAt(0);
							if(CacheTypeLoaded(depType))
								continue;
							try
							{
								Type depContType = CreateContextType(depType, dependency);
								SetCacheType(depType, depContType);
							}
							catch(Exception ex)
							{
								ErrorCacheType(depType, ex);
								throw;
							}
						}
					}
					catch(Exception ex)
					{
						TypeCache<T>.ReadContextExcepted = new NotImplementedException("dependency fail", ex);
						throw;
					}
				}

				return TypeCache<T>.Read;
			}
		}
		
		internal static Type CreateContextType(Type targetType, List<Type> dependency)
		{
			if (targetType == typeof(Int32))
				return typeof(ReadContexts.Integer);
			if (targetType == typeof(UInt32))
				return typeof(ReadContexts.UInteger);
			
			//TODO: structure read context generate

			throw new NotImplementedException();
		}
		
		public static bool CacheTypeLoaded(Type targetType)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			Exception ex = typeCacheType.GetField("ReadContextExcepted").GetValue(null) as Exception;
			if(ex != null)
				throw ex;
			return typeCacheType.GetField("ReadContextType").GetValue(null) != null;
		}
		
		private static void ErrorCacheType(Type targetType, Exception ex)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			typeCacheType.GetField("ReadContextExcepted").SetValue(null, ex);
		}
		
		private static void SetCacheType(Type targetType, Type readContextType)
		{
			Type typeCacheType = typeof(TypeCache<>).MakeGenericType(targetType);
			typeCacheType.GetField("ReadContextType").SetValue(null, readContextType);
		}
	}
}

