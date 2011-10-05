using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr2
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
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, len,
				0x48, 0x65, 0x6C, 0x6C,
				0x6F, 0x2C, 0x20, 0x77,
				0x6F, 0x72, 0x6C, 0x64,
				0x21, 0x00, 0x00, 0x00);
			
			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			string result = r.ReadVar<string>(30);
			Assert.AreEqual(readed, s.Position);
			Assert.IsNotNull(result);
			Assert.AreEqual(expected, result);
		}
		
		[TestCase(0, false)]
		[TestCase(1, true)]
		public void ReadBool(byte num, bool expected)
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, num);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);
			
			bool result = r.Read<bool>();
			Assert.AreEqual(4, s.Position);
			Assert.AreEqual(expected, result);
		}
		
		[TestCase(0, 4, null)]
		[TestCase(1, 8, 123)]
		public void ReadNullable(byte num, int readed, int? expected)
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, num,
				0x00, 0x00, 0x00, 123);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			int? result = r.Read<int?>();
			
			Assert.AreEqual(readed, s.Position);
			Assert.AreEqual(expected, result);
		}
	}
}

