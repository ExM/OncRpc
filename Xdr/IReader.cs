using System;

namespace Xdr
{
	public interface IReader
	{
		void Throw(Exception ex, Action<Exception> excepted);
		void ReadInt32(Action<Int32> completed, Action<Exception> excepted);
		void ReadUInt32(Action<UInt32> completed, Action<Exception> excepted);
		void ReadString(uint max, Action<string> completed, Action<Exception> excepted);
		void ReadFixOpaque(uint len, Action<byte[]> completed, Action<Exception> excepted);
		void ReadVarOpaque(uint max, Action<byte[]> completed, Action<Exception> excepted);

		void Read<T>(Action<T> completed, Action<Exception> excepted);
		void Read<T>(uint len, bool fix, Action<T> completed, Action<Exception> excepted);
	}
}

