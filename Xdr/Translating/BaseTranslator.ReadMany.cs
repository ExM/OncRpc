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
		private Delegate ReadManyBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				result = CreateReadManyForAttribute(targetType);
				if (result != null)
					return result;

				if (targetType == typeof(byte[]))
					return (Delegate)(ReadManyDelegate<byte[]>)ReadBytes;
				if (targetType == typeof(string))
					return (Delegate)(ReadManyDelegate<string>)ReadString;

				result = CreateArrayReader(targetType);
				if (result != null)
					return result;

				result = CreateListReader(targetType);
				if (result != null)
					return result;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadMany", targetType, typeof(ReadManyDelegate<>));
			}
		}

		public static Delegate CreateReadManyForAttribute(Type collectionType)
		{
			Type itemType = collectionType.GetKnownItemType();
			if (itemType == null)
				return null;

			Delegate attrResult = null;
			foreach (ReadManyAttribute attr in itemType.GetCustomAttributes(typeof(ReadManyAttribute), true)
				.Select((o) => (ReadManyAttribute)o))
			{
				Delegate candidate = attr.Create(collectionType);
				if (attrResult != null)
					throw new InvalidOperationException(string.Format("Duplicate methods in {0} ({1}.{2} & {2}.{3})",
						collectionType, candidate.Method.DeclaringType, candidate.Method.Name, attrResult.Method.DeclaringType, attrResult.Method.Name));
				attrResult = candidate;
			}

			if (attrResult == null)
				return null;
			else
				return attrResult;
		}

		public static Delegate CreateArrayReader(Type collectionType)
		{
			if (!collectionType.HasElementType)
				return null;
			Type itemType = collectionType.GetElementType();
			if (itemType == null || itemType.MakeArrayType() != collectionType)
				return null;
			
			MethodInfo mi = typeof(ArrayReader<>).MakeGenericType(itemType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}
		
		public static Delegate CreateListReader(Type collectionType)
		{
			if (!collectionType.IsGenericType)
				return null;
			
			Type genericType = collectionType.GetGenericTypeDefinition();
			if(genericType != typeof(List<>))
				return null;
			Type itemType = collectionType.GetGenericArguments()[0];
			
			MethodInfo mi = typeof(ListReader<>).MakeGenericType(itemType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(collectionType), mi);
		}

		private static void ReadBytes(IReader reader, uint len, bool fix, Action<byte[]> completed, Action<Exception> excepted)
		{
			if(fix)
				reader.ReadFixOpaque(len, completed, excepted);
			else
				reader.ReadVarOpaque(len, completed, excepted);
		}
		
		private static void ReadString(IReader reader, uint len, bool fix, Action<string> completed, Action<Exception> excepted)
		{
			reader.ReadString(len, completed, excepted);
		}
	}
}
