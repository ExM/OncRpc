using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr.Test
{
	[TestFixture]
	public class StandartReadersTest
	{
		[TestCase(0, 4, "")]
		[TestCase(1, 8, "H")]
		[TestCase(2, 8, "He")]
		[TestCase(3, 8, "Hel")]
		[TestCase(4, 8, "Hell")]
		[TestCase(13, 20, "Hello, world!")]
		public void ReadString(byte len, int readed, string expected)
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, len,
				0x48, 0x65, 0x6C, 0x6C,
				0x6F, 0x2C, 0x20, 0x77,
				0x6F, 0x72, 0x6C, 0x64,
				0x21, 0x00, 0x00, 0x00);
			s.Position = 0;

			ITranslator t = Translator.Create()
				.Build();

			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);

			string result = null;

			r.ReadVar<string>(30, (val) =>
			{
				result = val;
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(readed, s.Position);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);
		}
		
		[TestCase(0, false)]
		[TestCase(1, true)]
		public void ReadBool(byte num, bool expected)
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, num);
			s.Position = 0;

			ITranslator t = Translator.Create()
				.Build();

			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);

			bool? result = null;

			r.Read<bool>((val) =>
			{
				result = val;
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(4, s.Position);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result.Value);
		}
		
		[TestCase(0, 4, null)]
		[TestCase(1, 8, 123)]
		public void ReadNullable(byte num, int readed, int? expected)
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, num,
				0x00, 0x00, 0x00, 123);
			s.Position = 0;

			ITranslator t = Translator.Create()
				.Build();

			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);

			int? result = null;

			r.Read<int?>((val) =>
			{
				result = val;
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(readed, s.Position);
			Assert.AreEqual(expected, result);
		}
	}
}

