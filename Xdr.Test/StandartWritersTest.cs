using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr2
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
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);

			w.WriteVar<string>(5, text);
			Assert.AreEqual(expected, s.ToArray());
		}
		
		[TestCase(new int[0],     new byte[]{0x00, 0x00, 0x00, 0x00})]
		[TestCase(new int[]{123},    new byte[]{0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 123})]
		[TestCase(new int[]{123, 213},   new byte[]{0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 123, 0x00, 0x00, 0x00, 213})]
		public void WriteVarArray(int[] array, byte[] expected)
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);

			w.WriteVar<int[]>(5, array);
			Assert.AreEqual(expected, s.ToArray());
		}
	}
}

