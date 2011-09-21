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
		private Delegate WriteFixBuild(Type targetType)
		{
			try
			{
				Delegate result = null;
				
				if (targetType == typeof(byte[]))
					return (Delegate)(WriteManyDelegate<byte[]>)WriteFixBytes;

				result = CreateFixArrayWriter(targetType);
				if (result != null)
					return result;

				result = CreateFixListWriter(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return ErrorStub.WriteManyDelegate(targetType, ex);
			}
		}

		public static Delegate CreateFixListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if(itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}
		
		public static Delegate CreateFixArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void WriteFixBytes(IWriter writer, byte[] items, uint len, Action completed, Action<Exception> excepted)
		{
			writer.WriteFixOpaque(items, len, completed, excepted);
		}
	}
}
