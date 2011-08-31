using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Translating
{
	internal sealed class Writer : IWriter
	{
		private BaseTranslator _translator;
		private IByteWriter _writer;

		internal Writer(BaseTranslator translator, IByteWriter writer)
		{
			_translator = translator;
			_writer = writer;
		}
		
		public void Throw(Exception ex, Action<Exception> excepted)
		{
			_writer.Throw(ex, excepted);
		}

		public void WriteInt32(int item, Action completed, Action<Exception> excepted)
		{
			_writer.Write(XdrEncoding.EncodeInt32(item), completed, excepted);
		}

		public void WriteUInt32(uint item, Action completed, Action<Exception> excepted)
		{
			_writer.Write(XdrEncoding.EncodeUInt32(item), completed, excepted);
		}

		public void WriteInt64(long item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteUInt64(ulong item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteSingle(float item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteDouble(double item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteString(string item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteFixOpaque(byte[] item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void WriteVarOpaque(byte[] item, Action completed, Action<Exception> excepted)
		{
			throw new NotImplementedException ();
		}

		public void Write<T>(T item, Action completed, Action<Exception> excepted)
		{
			_translator.Write<T>(this, item, completed, excepted);
		}

		public void Write<T>(T items, bool fix, Action completed, Action<Exception> excepted)
		{
			_translator.Write<T>(this, items, fix, completed, excepted);
		}
	}
}
