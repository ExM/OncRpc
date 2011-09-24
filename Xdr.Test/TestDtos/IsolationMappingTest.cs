using System;
using System.Linq;
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

			ITranslator t1 = Translator.Create()
				.Map<SimplyInt>(SimplyInt.Read2)
				.Build();


			Reader r1 = t1.CreateReader(ss);

			s.Position = 0;
			r1.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(-7, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));


			ITranslator t2 = Translator.Create()
				.Map<SimplyInt>(SimplyInt.Read)
				.Build();

			Reader r2 = t2.CreateReader(ss);

			s.Position = 0;
			r2.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(7, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
		}
	}
}

