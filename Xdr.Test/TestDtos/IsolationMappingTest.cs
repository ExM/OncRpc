using System;
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Xdr2.TestDtos;

namespace Xdr2
{
	[TestFixture]
	public class IsolationMappingTest
	{
		[Test]
		public void Read()
		{
			ByteReader s1 = new ByteReader(
				0x00, 0x00, 0x00, 0x07,
				0xCD, 0xEF, 0x98, 0x76);

			ReadBuilder builder1 = new ReadBuilder()
				.Map<SimplyInt>(SimplyInt.Read2);
			Reader r1 = builder1.Create(s1);

			var val1 = r1.Read<SimplyInt>();
			Assert.AreEqual(-7, val1.Field1);
			Assert.AreEqual(0xCDEF9876, val1.Field2);


			ByteReader s2 = new ByteReader(
				0x00, 0x00, 0x00, 0x07,
				0xCD, 0xEF, 0x98, 0x76);

			ReadBuilder builder2 = new ReadBuilder()
				.Map<SimplyInt>(SimplyInt.Read);
			Reader r2 = builder2.Create(s2);

			var val2 = r2.Read<SimplyInt>();
			Assert.AreEqual(7, val2.Field1);
			Assert.AreEqual(0xCDEF9876, val2.Field2);
		}
	}
}

