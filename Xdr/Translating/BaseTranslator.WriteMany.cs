using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.WriteContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
	{
		private Delegate WriteManyBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				//TODO: write many build

				//result = CreateWriteManyForAttribute(targetType);
				//if (result != null)
				//	return result;

				if (targetType == typeof(byte[]))
					return (Delegate)(WriteManyDelegate<byte[]>)WriteBytes;

				result = CreateArrayWriter(targetType);
				if (result != null)
					return result;

				result = CreateListWriter(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "WriteMany", targetType, typeof(WriteManyDelegate<>));
			}
		}

		public static Delegate CreateListWriter(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;

			Type genericType = collectionType.GetGenericTypeDefinition();
			if (genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateArrayWriter(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void WriteBytes(IWriter writer, byte[] items, bool fix, Action completed, Action<Exception> excepted)
		{
			if (fix)
				writer.WriteFixOpaque(items, completed, excepted);
			else
				writer.WriteVarOpaque(items, completed, excepted);
		}
	}
}
