using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public abstract class Reader
	{
		public readonly IByteReader ByteReader;

		protected Reader(IByteReader reader)
		{
			ByteReader = reader;
		}

		public T Read<T>()
		{
			try
			{
				return CacheRead<T>();
			}
			catch (SystemException ex)
			{
				throw MapException.ReadOne(typeof(T), ex);
			}
		}

		protected abstract T CacheRead<T>();

		public T ReadFix<T>(uint len)
		{
			try
			{
				return CacheReadFix<T>(len);
			}
			catch (SystemException ex)
			{
				throw MapException.ReadFix(typeof(T), len, ex);
			}
		}

		protected abstract T CacheReadFix<T>(uint len);

		public T ReadVar<T>(uint max)
		{
			try
			{
				return CacheReadVar<T>(max);
			}
			catch (SystemException ex)
			{
				throw MapException.ReadVar(typeof(T), max, ex);
			}
		}

		protected abstract T CacheReadVar<T>(uint max);
		
		public T ReadOption<T>() where T: class
		{
			if (Read<bool>())
				return Read<T>();
			else
				return null;
		}
	}
}
