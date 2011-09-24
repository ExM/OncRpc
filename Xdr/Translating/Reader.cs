using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public sealed class Reader
	{
		private BaseTranslator _translator;
		private IByteReader _reader;

		internal Reader(BaseTranslator translator, IByteReader reader)
		{
			_translator = translator;
			_reader = reader;
		}
		
		public void Throw(Exception ex, Action<Exception> excepted)
		{
			_reader.Throw(ex, excepted);
		}

		public void ReadInt32(Action<int> completed, Action<Exception> excepted)
		{
			_reader.Read(4,
				(bytes) => completed(XdrEncoding.DecodeInt32(bytes)),
				excepted);
		}

		public void ReadUInt32(Action<uint> completed, Action<Exception> excepted)
		{
			_reader.Read(4,
				(bytes) => completed(XdrEncoding.DecodeUInt32(bytes)),
				excepted);
		}

		public void ReadInt64(Action<long> completed, Action<Exception> excepted)
		{
			_reader.Read(8,
				(bytes) => completed(XdrEncoding.DecodeInt64(bytes)),
				excepted);
		}

		public void ReadUInt64(Action<ulong> completed, Action<Exception> excepted)
		{
			_reader.Read(8,
				(bytes) => completed(XdrEncoding.DecodeUInt64(bytes)),
				excepted);
		}

		public void ReadSingle(Action<float> completed, Action<Exception> excepted)
		{
			_reader.Read(4,
				(bytes) => completed(XdrEncoding.DecodeSingle(bytes)),
				excepted);
		}

		public void ReadDouble(Action<double> completed, Action<Exception> excepted)
		{
			_reader.Read(8,
				(bytes) => completed(XdrEncoding.DecodeDouble(bytes)),
				excepted);
		}

		public void ReadString(uint max, Action<string> completed, Action<Exception> excepted)
		{
			ReadVarOpaque(max,
				(bytes) => completed(Encoding.ASCII.GetString(bytes)),
				excepted);
		}

		public void ReadFixOpaque(uint len, Action<byte[]> completed, Action<Exception> excepted)
		{
			_reader.Read(len, (bytes) =>
			{
				if (len % 4u == 0)
					completed(bytes);
				else
					_reader.Read(4u - len % 4u, (tail) => completed(bytes), excepted);
			}, excepted);
		}

		public void ReadVarOpaque(uint max, Action<byte[]> completed, Action<Exception> excepted)
		{
			ReadUInt32((len) =>
			{
				if (len == 0)
					completed(new byte[0]);
				else if (len <= max)
					ReadFixOpaque(len, completed, excepted);
				else
					excepted(new InvalidOperationException("unexpected length"));
			}, excepted);
		}

		public void Read<T>(Action<T> completed, Action<Exception> excepted)
		{
			_translator.Read<T>(this, completed, excepted);
		}

		public void ReadFix<T>(uint len, Action<T> completed, Action<Exception> excepted)
		{
			_translator.ReadFix<T>(this, len, completed, excepted);
		}
		
		public void ReadVar<T>(uint max, Action<T> completed, Action<Exception> excepted)
		{
			_translator.ReadVar<T>(this, max, completed, excepted);
		}
		
		public void ReadOption<T>(Action<T> completed, Action<Exception> excepted) where T: class
		{
			ReadUInt32((len) =>
			{
				if (len == 0)
					completed(null);
				else if (len == 1)
					Read<T>(completed, excepted);
				else
					excepted(new InvalidOperationException("unexpected option sign"));
			}, excepted);
		}
	}
}
