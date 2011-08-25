using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class OverrideMappingTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x07, 0xCD, 0xEF, 0x98, 0x76);
			s.Position = 0;
			
			ITranslator t = Translator.Create("test")
				.Map<SimplyIntAttr>(SimplyIntAttr_ReadContext2.Read)
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.Create(ss);
			
			r.Read<SimplyIntAttr>((val) =>
			{
				Assert.AreEqual(-7, val.Field1);
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
				.Map<SimplyIntAttr>(SimplyIntAttr_ReadContext2.Read)
				.Build();

			SyncStream ss = new SyncStream(s);
			IReader r = t.Create(ss);

			r.Read<List<SimplyIntAttr>>(2, true, (val) =>
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

