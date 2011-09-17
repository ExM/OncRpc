using System;

namespace Xdr
{
	public delegate void ReadOneDelegate<T>(IReader reader, Action<T> completed, Action<Exception> excepted);
	public delegate void ReadManyDelegate<T>(IReader reader, uint len, Action<T> completed, Action<Exception> excepted);
	public delegate void WriteOneDelegate<T>(IWriter writer, T item, Action completed, Action<Exception> excepted);
	public delegate void WriteManyDelegate<T>(IWriter writer, T items, bool fix, Action completed, Action<Exception> excepted);
}

