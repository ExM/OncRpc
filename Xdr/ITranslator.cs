using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface ITranslator
	{
		void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted);
		void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted);
		/*
		void Write<T>(IByteWriter writer, T obj, Action completed, Action<Exception> excepted);
		void Write<T>(IByteWriter writer, T list, uint len, bool fix, Action completed, Action<Exception> excepted);
		*/
	}
}
