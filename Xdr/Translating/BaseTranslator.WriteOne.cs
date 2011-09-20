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
		private Delegate WriteOneBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				result = CreateEnumWriter(targetType);
				if (result != null)
					return result;

				result = CreateNullableWriter(targetType);
				if (result != null)
					return result;
				
				result = CreateFixArrayWriter(targetType);
				if (result != null)
					return result;
				
				result = CreateFixListWriter(targetType);
				if (result != null)
					return result;

				if (targetType == typeof(Int32))
					return (Delegate)(WriteOneDelegate<Int32>)WriteInt32;
				if (targetType == typeof(UInt32))
					return (Delegate)(WriteOneDelegate<UInt32>)WriteUInt32;
				if (targetType == typeof(Int64))
					return (Delegate)(WriteOneDelegate<Int64>)WriteInt64;
				if (targetType == typeof(UInt64))
					return (Delegate)(WriteOneDelegate<UInt64>)WriteUInt64;
				if (targetType == typeof(Single))
					return (Delegate)(WriteOneDelegate<Single>)WriteSingle;
				if (targetType == typeof(Double))
					return (Delegate)(WriteOneDelegate<Double>)WriteDouble;
				if (targetType == typeof(bool))
					return (Delegate)(WriteOneDelegate<bool>)WriteBool;
				if (targetType == typeof(string))
					return (Delegate)(WriteOneDelegate<string>)WriteString;
				if (targetType == typeof(byte[]))
					return (Delegate)(WriteOneDelegate<byte[]>)WriteFixBytes;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "Write", targetType, typeof(WriteOneDelegate<>));
			}
		}
		
		public static Delegate CreateFixListWriter(Type collectionType)
		{
			Type itemType = collectionType.ListSubType();
			if(itemType == null)
				return null;

			MethodInfo mi = typeof(ListWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(collectionType), mi);
		}
		
		public static Delegate CreateFixArrayWriter(Type collectionType)
		{
			Type itemType = collectionType.ArraySubType();
			if (itemType == null)
				return null;

			MethodInfo mi = typeof(ArrayWriter<>).MakeGenericType(itemType).GetMethod("WriteFix", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(collectionType), mi);
		}

		public static Delegate CreateEnumWriter(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;

			MethodInfo mi = typeof(EnumWriter<>).MakeGenericType(targetType).GetMethod("Write", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static Delegate CreateNullableWriter(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if(itemType == null)
				return null;

			MethodInfo mi = typeof(BaseTranslator).GetMethod("WriteNullable", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(WriteOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void WriteNullable<T>(IWriter writer, T? item, Action completed, Action<Exception> excepted) where T: struct
		{
			if (item.HasValue)
				writer.WriteUInt32(1, () => writer.Write<T>(item.Value, completed, excepted), excepted);
			else
				writer.WriteUInt32(0, completed, excepted);
		}

		private static void WriteInt32(IWriter writer, int item, Action completed, Action<Exception> excepted)
		{
			writer.WriteInt32(item, completed, excepted);
		}

		private static void WriteUInt32(IWriter writer, uint item, Action completed, Action<Exception> excepted)
		{
			writer.WriteUInt32(item, completed, excepted);
		}

		private static void WriteInt64(IWriter writer, long item, Action completed, Action<Exception> excepted)
		{
			writer.WriteInt64(item, completed, excepted);
		}

		private static void WriteUInt64(IWriter writer, ulong item, Action completed, Action<Exception> excepted)
		{
			writer.WriteUInt64(item, completed, excepted);
		}

		private static void WriteSingle(IWriter writer, float item, Action completed, Action<Exception> excepted)
		{
			writer.WriteSingle(item, completed, excepted);
		}

		private static void WriteDouble(IWriter writer, double item, Action completed, Action<Exception> excepted)
		{
			writer.WriteDouble(item, completed, excepted);
		}

		private static void WriteBool(IWriter writer, bool item, Action completed, Action<Exception> excepted)
		{
			if(item)
				writer.WriteUInt32(1, completed, excepted);
			else
				writer.WriteUInt32(0, completed, excepted);
		}

		private static void WriteString(IWriter writer, string item, Action completed, Action<Exception> excepted)
		{
			writer.WriteString(item, completed, excepted);
		}
		
		private static void WriteFixBytes(IWriter writer, byte[] items, Action completed, Action<Exception> excepted)
		{
			writer.WriteFixOpaque(items, completed, excepted);
		}
	}
}
