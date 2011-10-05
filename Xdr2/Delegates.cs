using System;

namespace Xdr2
{
	public delegate T ReadOneDelegate<T>(Reader reader);
	public delegate T ReadManyDelegate<T>(Reader reader, uint len);
	public delegate void WriteOneDelegate<T>(Writer writer, T item);
	public delegate void WriteManyDelegate<T>(Writer writer, uint len, T item);
}

