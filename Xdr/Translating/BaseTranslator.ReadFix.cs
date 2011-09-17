using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.ReadContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
	{
		private Delegate ReadFixBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				if (targetType == typeof(byte[]))
					return (Delegate)(ReadManyDelegate<byte[]>)ReadFixBytes;

				result = CreateFixArrayReader(targetType);
				if (result != null)
					return result;

				result = CreateFixListReader(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadFix", targetType, typeof(ReadManyDelegate<>));
			}
		}

		public static Delegate CreateFixArrayReader(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;
			
			MethodInfo mi = typeof(FixArrayReader<>).MakeGenericType(itemType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}
		
		public static Delegate CreateFixListReader(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;
			
			Type genericType = collectionType.GetGenericTypeDefinition();
			if(genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];
			
			MethodInfo mi = typeof(FixListReader<>).MakeGenericType(itemType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void ReadFixBytes(IReader reader, uint len, Action<byte[]> completed, Action<Exception> excepted)
		{
			reader.ReadFixOpaque(len, completed, excepted);
		}
	}
}
