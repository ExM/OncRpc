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
		private Delegate ReadOneBuild(Type targetType)
		{
			try
			{
				Delegate result = null;

				result = CreateEnumDelegate(targetType);
				if (result != null)
					return result;

				result = CreateNullableReader(targetType);
				if (result != null)
					return result;

				if (targetType == typeof(Int32))
					return (Delegate)(ReadOneDelegate<Int32>)ReadInt32;
				if (targetType == typeof(UInt32))
					return (Delegate)(ReadOneDelegate<UInt32>)ReadUInt32;
				if (targetType == typeof(Int64))
					return (Delegate)(ReadOneDelegate<Int64>)ReadInt64;
				if (targetType == typeof(UInt64))
					return (Delegate)(ReadOneDelegate<UInt64>)ReadUInt64;
				if (targetType == typeof(Single))
					return (Delegate)(ReadOneDelegate<Single>)ReadSingle;
				if (targetType == typeof(Double))
					return (Delegate)(ReadOneDelegate<Double>)ReadDouble;
				if (targetType == typeof(bool))
					return (Delegate)(ReadOneDelegate<bool>)BoolReader.Read;

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return CreateStubDelegate(ex, "ReadOne", targetType, typeof(ReadOneDelegate<>));
			}
		}

		public static Delegate CreateNullableReader(Type targetType)
		{
			if (!targetType.IsGenericType)
				return null;
			if (targetType.GetGenericTypeDefinition() != typeof(Nullable<>))
				return null;
			Type itemType = targetType.GetGenericArguments()[0];

			MethodInfo mi = typeof(NullableReader<>).MakeGenericType(itemType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static Delegate CreateEnumDelegate(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(EnumReader<>).MakeGenericType(targetType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}
		
		private static void ReadInt32(IReader reader, Action<int> completed, Action<Exception> excepted)
		{
			reader.ReadInt32(completed, excepted);
		}
		
		private static void ReadUInt32(IReader reader, Action<uint> completed, Action<Exception> excepted)
		{
			reader.ReadUInt32(completed, excepted);
		}

		private static void ReadInt64(IReader reader, Action<long> completed, Action<Exception> excepted)
		{
			reader.ReadInt64(completed, excepted);
		}

		private static void ReadUInt64(IReader reader, Action<ulong> completed, Action<Exception> excepted)
		{
			reader.ReadUInt64(completed, excepted);
		}

		private static void ReadSingle(IReader reader, Action<float> completed, Action<Exception> excepted)
		{
			reader.ReadSingle(completed, excepted);
		}

		private static void ReadDouble(IReader reader, Action<double> completed, Action<Exception> excepted)
		{
			reader.ReadDouble(completed, excepted);
		}
	}
}
