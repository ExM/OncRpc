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
		private static object _sync = new object();
		private static Dictionary<Type, EmitResult> _readerCache = new Dictionary<Type, EmitResult>();
		private static Dictionary<Type, EmitResult> _writerCache = new Dictionary<Type, EmitResult>();
		
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
						result.Method = EmitReader(targetType);
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
						result.Method = EmitWriter(targetType);
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
		
		public static Delegate EmitReader(Type targetType)
		{
			OrderModel ordModel = OrderModel.Create(targetType);
			SwitchModel swModel = SwitchModel.Create(targetType);

			if (swModel != null && ordModel != null)
				throw new InvalidOperationException("unknown way to convert");

			if (swModel != null)
				return swModel.BuildReader(targetType);

			if (ordModel != null)
				return ordModel.BuildReader(targetType);

			return null;
		}
		
		public static Delegate EmitWriter(Type targetType)
		{
			OrderModel ordModel = OrderModel.Create(targetType);
			SwitchModel swModel = SwitchModel.Create(targetType);

			if (swModel != null && ordModel != null)
				throw new InvalidOperationException("unknown way to convert");

			if (swModel != null)
				return swModel.BuildWriter(targetType);

			if (ordModel != null)
				return ordModel.BuildWriter(targetType);

			return null;
		}
	}
}

