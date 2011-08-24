using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;

namespace Xdr
{
	[TestFixture]
	public class SimplyIntTest
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
			IReader r = t.Create(ss);
			
			r.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(0x1234ABCD, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
		}
	}
}

