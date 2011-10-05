using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Xdr2.TestDtos;

namespace Xdr2
{
	[TestFixture]
	public class DirectMappingTest
	{
		[Test]
		public void Read()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder()
				.Map<SimplyInt>(SimplyInt.Read2);
			Reader r = builder.Create(s);

			var val = r.Read<SimplyInt>();
			Assert.AreEqual(-3, val.Field1);
			Assert.AreEqual(4u, val.Field2);
			Assert.AreEqual(8, s.Position);
		}

		[Test]
		public void ReadList()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder()
				.Map<SimplyInt>(SimplyInt.Read2);
			Reader r = builder.Create(s);

			var val = r.ReadFix<List<SimplyInt>>(2);
			Assert.AreEqual(2, val.Count);
			Assert.AreEqual(-1, val[0].Field1);
			Assert.AreEqual( 2, val[0].Field2);
			Assert.AreEqual(-3, val[1].Field1);
			Assert.AreEqual( 4, val[1].Field2);
			Assert.AreEqual(16, s.Position);
		}
	}
}

