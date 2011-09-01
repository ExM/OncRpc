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
			_writer.Write(XdrEncoding.EncodeInt64(item), completed, excepted);
		}

		public void WriteUInt64(ulong item, Action completed, Action<Exception> excepted)
		{
			_writer.Write(XdrEncoding.EncodeUInt64(item), completed, excepted);
		}

		public void WriteSingle(float item, Action completed, Action<Exception> excepted)
		{
			_writer.Write(XdrEncoding.EncodeSingle(item), completed, excepted);
		}

		public void WriteDouble(double item, Action completed, Action<Exception> excepted)
		{
			_writer.Write(XdrEncoding.EncodeDouble(item), completed, excepted);
		}

		private Action _completed = null;
		private Action<Exception> _excepted = null;
		private byte[] _buffer = null;

		private static byte[][] _tail = new byte[][]
		{
			null,
			new byte[] { 0x00},
			new byte[] { 0x00, 0x00},
			new byte[] { 0x00, 0x00, 0x00}
		};

		public void WriteString(string item, Action completed, Action<Exception> excepted)
		{
			_completed = completed;
			_excepted = excepted;
			_buffer = Encoding.ASCII.GetBytes(item);
			_writer.Write(XdrEncoding.EncodeUInt32((uint)_buffer.LongLength), Len_Writed, excepted);
		}

		private void Len_Writed()
		{
			_writer.Write(_buffer, Buffer_Writed, _excepted);
		}

		private void Buffer_Writed()
		{
			uint tailLen = 4 - (uint)_buffer.LongLength % 4;
			if(tailLen == 0)
				_completed();
			else
				_writer.Write(_tail[tailLen], _completed, _excepted);
		}

		public void WriteFixOpaque(byte[] item, Action completed, Action<Exception> excepted)
		{
			_completed = completed;
			_excepted = excepted;
			_buffer = item;
			_writer.Write(_buffer, Buffer_Writed, _excepted);
		}

		public void WriteVarOpaque(byte[] item, Action completed, Action<Exception> excepted)
		{
			_completed = completed;
			_excepted = excepted;
			_buffer = item;
			_writer.Write(XdrEncoding.EncodeUInt32((uint)_buffer.LongLength), Len_Writed, excepted);
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
