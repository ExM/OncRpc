using System;

namespace Xdr
{
	public interface IWriter
	{
		void Throw(Exception ex, Action<Exception> excepted);
		void WriteInt32(Int32 item, Action completed, Action<Exception> excepted);
		void WriteUInt32(UInt32 item, Action completed, Action<Exception> excepted);
		void WriteString(string item, Action completed, Action<Exception> excepted);
		void WriteFixOpaque(byte[] item, Action completed, Action<Exception> excepted);
		void WriteVarOpaque(byte[] item, Action completed, Action<Exception> excepted);

		void Write<T>(T item, Action completed, Action<Exception> excepted);
		void Write<T>(T items, bool fix, Action<T> completed, Action<Exception> excepted);
	}
}

