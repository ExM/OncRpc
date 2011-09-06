using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class NullableReadingTest
	{
		[Test]
		public void ReadNull()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x02);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);
			
			r.Read<int?>((val) =>
			{
				Assert.IsFalse(val.HasValue);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(4, s.Position);
		}

		[Test]
		public void ReadItem()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);
			
			r.Read<int?>((val) =>
			{
				Assert.AreEqual(2, val);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(8, s.Position);
		}
	}
}

