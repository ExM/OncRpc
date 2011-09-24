using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class DirectMappingTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Map<SimplyInt>(SimplyInt.Read2)
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(-3, val.Field1);
				Assert.AreEqual(4u, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(8, s.Position);
		}

		[Test]
		public void ReadList()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;

			ITranslator t = Translator.Create()
				.Map<SimplyInt>(SimplyInt.Read2)
				.Build();

			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);

			r.ReadFix<List<SimplyInt>>(2, (val) =>
			{
				Assert.AreEqual(2, val.Count);
				Assert.AreEqual(-1, val[0].Field1);
				Assert.AreEqual(2, val[0].Field2);
				Assert.AreEqual(-3, val[1].Field1);
				Assert.AreEqual(4, val[1].Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(16, s.Position);
		}
	}
}

