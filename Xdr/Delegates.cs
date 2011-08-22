using System;

namespace Xdr
{
	public delegate void ReadOneDelegate<T>(IReader reader, Action<T> completed, Action<Exception> excepted);
	public delegate void ReadManyDelegate<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted);
	/*
	public delegate void WriteOneDelegate<T>(IByteWriter writer, T obj, Action completed, Action<Exception> excepted);
	public delegate void WriteManyDelegate<T>(IByteWriter writer, T list, uint len, bool fix, Action completed, Action<Exception> excepted);
	*/
}

