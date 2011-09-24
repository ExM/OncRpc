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

				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return ErrorStub.ReadOneDelegate(targetType, ex);
			}
		}

		public static Delegate CreateNullableReader(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if(itemType == null)
				return null;
			
			MethodInfo mi = typeof(BaseTranslator).GetMethod("ReadNullable", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		public static Delegate CreateEnumDelegate(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(EnumReader<>).MakeGenericType(targetType).GetMethod("Read", BindingFlags.Static | BindingFlags.Public);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}
		
		private static void ReadNullable<T>(IReader reader, Action<T?> completed, Action<Exception> excepted)
			where T: struct
		{
			reader.ReadUInt32((val) => 
			{
				if (val == 0)
					completed(null);
				else if(val == 1)
					reader.Read<T>((item) => completed(item), excepted);
				else
					excepted(new InvalidOperationException(string.Format("unexpected value {0}", val)));
			}, excepted);
		}
		
		private static void ReadBool(IReader reader, Action<bool> completed, Action<Exception> excepted)
		{
			reader.ReadInt32((val) =>
			{
				if (val == 0)
					completed(false);
				else if(val == 1)
					completed(true);
				else
					excepted(new InvalidCastException(string.Format("no boolean value `{0}'", val)));
			}, excepted);
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
