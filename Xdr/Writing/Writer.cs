using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public abstract class Writer
	{
		public readonly IByteWriter ByteWriter;

		protected Writer(IByteWriter writer)
		{
			ByteWriter = writer;
		}

		public void Write<T>(T item)
		{
			try
			{
				CacheWrite(item);
			}
			catch (SystemException ex)
			{
				throw MapException.WriteOne(typeof(T), ex);
			}
		}

		protected abstract void CacheWrite<T>(T item);

		public void WriteFix<T>(uint len, T item)
		{
			try
			{
				CacheWriteFix(len, item);
			}
			catch (SystemException ex)
			{
				throw MapException.WriteFix(typeof(T), len, ex);
			}
		}

		protected abstract void CacheWriteFix<T>(uint len, T item);

		public void WriteVar<T>(uint max, T item)
		{
			try
			{
				CacheWriteVar(max, item);
			}
			catch (SystemException ex)
			{
				throw MapException.WriteVar(typeof(T), max, ex);
			}
		}

		protected abstract void CacheWriteVar<T>(uint max, T item);
		
		public void WriteOption<T>(T item) where T: class
		{
			if(item == null)
				Write<bool>(false);
			else
			{
				Write<bool>(true);
				Write<T>(item);
			}
		}
	}
}
