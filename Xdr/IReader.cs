using System;

namespace Xdr
{
	public interface IReader
	{
		void Throw(Exception ex, Action<Exception> excepted);
		void ReadInt32(Action<Int32> completed, Action<Exception> excepted);
		void ReadUInt32(Action<UInt32> completed, Action<Exception> excepted);
		void ReadInt64(Action<Int64> completed, Action<Exception> excepted);
		void ReadUInt64(Action<UInt64> completed, Action<Exception> excepted);
		void ReadSingle(Action<Single> completed, Action<Exception> excepted);
		void ReadDouble(Action<Double> completed, Action<Exception> excepted);

		void ReadString(uint max, Action<string> completed, Action<Exception> excepted);
		void ReadFixOpaque(uint len, Action<byte[]> completed, Action<Exception> excepted);
		void ReadVarOpaque(uint max, Action<byte[]> completed, Action<Exception> excepted);

		void Read<T>(Action<T> completed, Action<Exception> excepted);
		void ReadFix<T>(uint len, Action<T> completed, Action<Exception> excepted);
		void ReadVar<T>(uint max, Action<T> completed, Action<Exception> excepted);
	}
}

