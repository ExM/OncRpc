using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr.Examples
{
	internal class Reader : IReader
	{
		private ITranslator _translator;
		private IByteReader _reader;

		internal Reader(ITranslator translator, IByteReader reader)
		{
			_translator = translator;
			_reader = reader;
		}

		private uint _maxLen;
		private uint _len = 0;
		private byte[] _bytes = null;
		private Action<byte[]> _completed_bytes;
		private Action<string> _completed_string;
		private Action<Int32> _completed_Int32;
		private Action<UInt32> _completed_UInt32;
		private Action<Exception> _excepted;

		#region Int32

		private void Int32_Readed(byte[] val)
		{
			_completed_Int32(XdrEncoding.DecodeInt32(val));
		}

		public void ReadInt32(Action<int> completed, Action<Exception> excepted)
		{
			_completed_Int32 = completed;
			_reader.Read(4, Int32_Readed, excepted);
		}

		#endregion Int32

		#region UInt32

		private void UInt32_Readed(byte[] val)
		{
			_completed_UInt32(XdrEncoding.DecodeUInt32(val));
		}

		public void ReadUInt32(Action<uint> completed, Action<Exception> excepted)
		{
			_completed_UInt32 = completed;
			_reader.Read(4, UInt32_Readed, excepted);
		}

		#endregion UInt32

		#region String

		private void String_Length_Readed(byte[] val)
		{
			_len = XdrEncoding.DecodeUInt32(val);
			
			if(_len == 0)
				_completed_string(string.Empty);
			else if (_len <= _maxLen)
				_reader.Read(_len, String_Readed, _excepted);
			else
				_excepted(new InvalidOperationException("unexpected length"));
		}

		private void String_Readed(byte[] val)
		{
			_bytes = val;

			if(_len % 4 == 0)
				_completed_string(Encoding.ASCII.GetString(_bytes));
			else
				_reader.Read((uint)(4 - _len % 4), String_Tail_Readed, _excepted);
		}

		private void String_Tail_Readed(byte[] val)
		{
			_completed_string(Encoding.ASCII.GetString(_bytes));
		}

		public void ReadString(uint max, Action<string> completed, Action<Exception> excepted)
		{
			_maxLen = max;
			_completed_string = completed;
			_excepted = excepted;
			_reader.Read(4, String_Length_Readed, _excepted);
		}

		#endregion String

		#region FixOpaque

		private void Bytes_Readed(byte[] val)
		{
			_bytes = val;
			
			if(_len % 4 == 0)
				_completed_bytes(_bytes);
			else
				_reader.Read((uint)(_len % 4), Bytes_Tail_Readed, _excepted);
		}

		private void Bytes_Tail_Readed(byte[] val)
		{
			_completed_bytes(_bytes);
		}

		public void ReadFixOpaque(uint len, Action<byte[]> completed, Action<Exception> excepted)
		{
			_len = len;
			_completed_bytes = completed;
			_excepted = excepted;
			_reader.Read(_len, Bytes_Readed, _excepted);
		}

		#endregion FixOpaque

		#region VarOpaque

		private void Bytes_Length_Readed(byte[] val)
		{
			_len = XdrEncoding.DecodeUInt32(val);
		
			if (_len > _maxLen)
				_excepted(new InvalidOperationException("unexpected length"));
			else
				_reader.Read(_len, Bytes_Readed, _excepted);
		}

		public void ReadVarOpaque(uint max, Action<byte[]> completed, Action<Exception> excepted)
		{
			_maxLen = max;
			_completed_bytes = completed;
			_excepted = excepted;
			_reader.Read(4, Bytes_Length_Readed, _excepted);
		}

		#endregion VarOpaque

		public void Read<T>(Action<T> completed, Action<Exception> excepted)
		{
			_translator.Read<T>(this, completed, excepted);
		}

		public void Read<T>(uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			_translator.Read<T>(this, len, fix, completed, excepted);
		}
	}
}
