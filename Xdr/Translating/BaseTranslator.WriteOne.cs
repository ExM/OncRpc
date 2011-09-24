﻿using System;
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
				
				throw new NotImplementedException(string.Format("unknown type {0}", targetType.FullName));
			}
			catch (Exception ex)
			{
				return ErrorStub.WriteOneDelegate(targetType, ex);
			}
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

		private static void WriteNullable<T>(Writer writer, T? item, Action completed, Action<Exception> excepted) where T: struct
		{
			if (item.HasValue)
				writer.WriteUInt32(1, () => writer.Write<T>(item.Value, completed, excepted), excepted);
			else
				writer.WriteUInt32(0, completed, excepted);
		}
	}
}
