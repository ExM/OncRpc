using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr2
{
	[TestFixture]
	public class ArrayReadingTest
	{
		[Test]
		public void ReadFixList()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);
			
			var val = r.ReadFix<List<int>>(3);
			Assert.AreEqual(3, val.Count);
			Assert.AreEqual(1, val[0]);
			Assert.AreEqual(2, val[1]);
			Assert.AreEqual(3, val[2]);
			Assert.AreEqual(12, s.Position);
		}

		[Test]
		public void ReadVarList()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.ReadVar<List<int>>(4);
			Assert.AreEqual(3, val.Count);
			Assert.AreEqual(2, val[0]);
			Assert.AreEqual(3, val[1]);
			Assert.AreEqual(4, val[2]);
			Assert.AreEqual(16, s.Position);
		}
		
		[Test]
		public void ReadFixArray()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.ReadFix<int[]>(3);
			Assert.AreEqual(3, val.Length);
			Assert.AreEqual(1, val[0]);
			Assert.AreEqual(2, val[1]);
			Assert.AreEqual(3, val[2]);
			Assert.AreEqual(12, s.Position);
		}
		
		[Test]
		public void ReadVarArray()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.ReadVar<int[]>(4);
			Assert.AreEqual(3, val.Length);
			Assert.AreEqual(2, val[0]);
			Assert.AreEqual(3, val[1]);
			Assert.AreEqual(4, val[2]);
			Assert.AreEqual(16, s.Position);
		}
	}
}

