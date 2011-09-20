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
		private Delegate WriteVarBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				if (targetType == typeof(byte[]))
					return (Delegate)(WriteOneDelegate<byte[]>)WriteVarBytes;

				result = CreateVarArrayWriter(targetType);
				if (result != null)
					return result;

				result = CreateVarListWriter(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "Write", targetType, typeof(WriteOneDelegate<>));
			}
		}

		public static Delegate CreateVarListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateVarArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void WriteVarBytes(IWriter writer, byte[] items, Action completed, Action<Exception> excepted)
		{
			writer.WriteVarOpaque(items, completed, excepted);
		}
	}
}
