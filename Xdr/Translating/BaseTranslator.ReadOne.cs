using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xdr.Translating;
using Xdr.ReadContexts;
using Xdr.EmitContexts;

namespace Xdr
{
	public abstract partial class BaseTranslator: ITranslator
	{
		public static Delegate CreateEnumDelegate(Type targetType)
		{
			if (!targetType.IsEnum)
				return null;
			MethodInfo mi = typeof(BaseTranslator).GetMethod("EnumRead", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(targetType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}
		
		private static void EnumRead<T>(Reader reader, Action<T> completed, Action<Exception> excepted) where T: struct
		{
			reader.ReadInt32((val) => EnumHelper<T>.IntToEnum(val, completed, excepted), excepted);
		}
		
		public static Delegate CreateNullableReader(Type targetType)
		{
			Type itemType = targetType.NullableSubType();
			if(itemType == null)
				return null;
			
			MethodInfo mi = typeof(BaseTranslator).GetMethod("ReadNullable", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(itemType);
			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), mi);
		}

		private static void ReadNullable<T>(Reader reader, Action<T?> completed, Action<Exception> excepted)
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
	}
}
