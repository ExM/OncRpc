using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Xdr.Test.TestDtos;

namespace Xdr
{
	[TestFixture]
	public class EnumTest
	{
		[Test, ExpectedException(typeof(MapException))]
		public void ExceptionRead()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x00);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			r.Read<IntEnum>();
		}

		[Test]
		public void ReadOne()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			Assert.AreEqual(IntEnum.One, r.Read<IntEnum>());
			Assert.AreEqual(4, s.Position);
		}

		[Test]
		public void ReadTwo()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x02);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			Assert.AreEqual(IntEnum.Two, r.Read<IntEnum>());
			Assert.AreEqual(4, s.Position);
		}
		
		[Test]
		public void ReadByteOne()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			Assert.AreEqual(ByteEnum.One, r.Read<ByteEnum>());
			Assert.AreEqual(4, s.Position);
		}
	}
}

