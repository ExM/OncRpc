using System;
using NUnit.Framework;
using System.IO;
using Xdr.Example;

namespace Xdr.Test
{
	[TestFixture]
	public class IntTreeTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(new byte[] { 0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76 }, 0, 8);
			s.Position = 0;

			SyncStream ss = new SyncStream(s);

			XdrReader<IntTree>.Read(ss, (val) =>
			{
				Assert.AreEqual(0x1234ABCD, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
	}
}

