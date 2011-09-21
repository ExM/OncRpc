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
				
				if (targetType == typeof(string))
					return (Delegate)(WriteManyDelegate<string>)WriteString;
				if (targetType == typeof(byte[]))
					return (Delegate)(WriteManyDelegate<byte[]>)WriteVarBytes;

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
				return ErrorStub.WriteManyDelegate(targetType, ex);
			}
		}

		public static Delegate CreateVarListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateVarArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteVar", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void WriteVarBytes(IWriter writer, byte[] items, uint max, Action completed, Action<Exception> excepted)
		{
			writer.WriteVarOpaque(items, max, completed, excepted);
		}
		
		private static void WriteString(IWriter writer, string item, uint max, Action completed, Action<Exception> excepted)
		{
			writer.WriteString(item, max, completed, excepted);
		}
	}
}
