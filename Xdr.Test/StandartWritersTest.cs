using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr.Test
{
	[TestFixture]
	public class StandartWritersTest
	{
		[TestCase("",     new byte[]{0x00, 0x00, 0x00, 0x00})]
		[TestCase("H",    new byte[]{0x00, 0x00, 0x00, 0x01, 0x48, 0x00, 0x00, 0x00})]
		[TestCase("He",   new byte[]{0x00, 0x00, 0x00, 0x02, 0x48, 0x65, 0x00, 0x00})]
		[TestCase("Hel",  new byte[]{0x00, 0x00, 0x00, 0x03, 0x48, 0x65, 0x6C, 0x00})]
		[TestCase("Hell", new byte[]{0x00, 0x00, 0x00, 0x04, 0x48, 0x65, 0x6C, 0x6C})]
		public void WriteString(string text, byte[] expected)
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IWriter w = t.CreateWriter(ss);

			w.WriteVar<string>(text, 5,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(expected, s.ToArray());
		}
		
		[TestCase(new int[0],     new byte[]{0x00, 0x00, 0x00, 0x00})]
		[TestCase(new int[]{123},    new byte[]{0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 123})]
		[TestCase(new int[]{123, 213},   new byte[]{0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 123, 0x00, 0x00, 0x00, 213})]
		public void WriteVarArray(int[] array, byte[] expected)
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IWriter w = t.CreateWriter(ss);

			w.WriteVar<int[]>(array, 5,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(expected, s.ToArray());
		}
	}
}

