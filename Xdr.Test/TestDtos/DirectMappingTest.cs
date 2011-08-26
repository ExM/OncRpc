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
			s.Write(0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76);
			s.Position = 0;
			
			ITranslator t = Translator.Create("test")
				.Map<SimplyInt>(SimplyInt_ReadContext.Read)
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);
			
			r.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(0x1234ABCD, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
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

			ITranslator t = Translator.Create("test")
				.Map<SimplyInt>(SimplyInt_ReadContext.Read)
				.Map<List<SimplyInt>>(SimplyInt_ReadListContext.ReadList)
				.Build();

			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			r.Read<List<SimplyInt>>(2, true, (val) =>
			{
				Assert.AreEqual(2, val.Count);
				Assert.AreEqual(1, val[0].Field1);
				Assert.AreEqual(2, val[0].Field2);
				Assert.AreEqual(3, val[1].Field1);
				Assert.AreEqual(4, val[1].Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(16, s.Position);
		}
	}
}

