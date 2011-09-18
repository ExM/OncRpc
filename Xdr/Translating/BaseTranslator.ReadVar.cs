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
		private Delegate ReadVarBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				if (targetType == typeof(byte[]))
					return (Delegate)(ReadManyDelegate<byte[]>)ReadVarBytes;
				if (targetType == typeof(string))
					return (Delegate)(ReadManyDelegate<string>)ReadString;

				result = CreateVarArrayReader(targetType);
				if (result != null)
					return result;

				result = CreateVarListReader(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadVar", targetType, typeof(ReadManyDelegate<>));
			}
		}

		public static Delegate CreateVarArrayReader(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;
			
			MethodInfo mi = typeof(ArrayReader<>).MakeGenericType(itemType).GetMethod("ReadVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}
		
		public static Delegate CreateVarListReader(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;
			
			Type genericType = collectionType.GetGenericTypeDefinition();
			if(genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];
			
			MethodInfo mi = typeof(ListReader<>).MakeGenericType(itemType).GetMethod("ReadVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void ReadVarBytes(IReader reader, uint len, Action<byte[]> completed, Action<Exception> excepted)
		{
			reader.ReadVarOpaque(len, completed, excepted);
		}
		
		private static void ReadString(IReader reader, uint len, Action<string> completed, Action<Exception> excepted)
		{
			reader.ReadString(len, completed, excepted);
		}
	}
}
