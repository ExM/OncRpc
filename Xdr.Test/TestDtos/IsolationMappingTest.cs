using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class IsolationMappingTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x07, 0xCD, 0xEF, 0x98, 0x76);
			SyncStream ss = new SyncStream(s);

			ITranslator t1 = Translator.Create("test")
				.Map<SimplyIntAttr>(SimplyIntAttr_ReadContext2.Read)
				.Build();
			
			
			IReader r1 = t1.Create(ss);

			s.Position = 0;
			r1.Read<SimplyIntAttr>((val) =>
			{
				Assert.AreEqual(-7, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));


			ITranslator t2 = Translator.Create("test")
				.Build();

			IReader r2 = t2.Create(ss);

			s.Position = 0;
			r2.Read<SimplyIntAttr>((val) =>
			{
				Assert.AreEqual(7, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
		}
	}
}

