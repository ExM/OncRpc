using System;

namespace Xdr
{
	public delegate void ReadOneDelegate<T>(Reader reader, Action<T> completed, Action<Exception> excepted);
	public delegate void ReadManyDelegate<T>(Reader reader, uint len, Action<T> completed, Action<Exception> excepted);
	public delegate void WriteOneDelegate<T>(Writer writer, T item, Action completed, Action<Exception> excepted);
	public delegate void WriteManyDelegate<T>(Writer writer, T item, uint len, Action completed, Action<Exception> excepted);
}

