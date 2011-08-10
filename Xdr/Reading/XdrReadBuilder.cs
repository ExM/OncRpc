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
					try
					{
						TypeCache<T>.ReadContextType = CreateContextType(typeof(T));
					}
					catch(Exception ex)
					{
						TypeCache<T>.ReadContextExcepted = ex;
						throw;
					}
				}

				return TypeCache<T>.Read;
			}
		}
		
		internal static Type CreateContextType(Type targetType, List<Type> dependency)
		{
			if (targetType == typeof(Int32))
				return typeof(Int32_ReadContext);


			throw new NotImplementedException();
			
			//return null;
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

