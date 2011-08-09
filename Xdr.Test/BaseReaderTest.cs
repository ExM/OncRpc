using System;
using NUnit.Framework;
using System.IO;
using Xdr.Example;

namespace Xdr.Test
{
	[TestFixture]
	public class BaseReaderTest
	{
		[Test]
		public void ReadInt32()
		{
			MemoryStream s = new MemoryStream();
			s.Write(new byte[] { 0x12, 0x34, 0xAB, 0xCD}, 0, 4);
			s.Position = 0;

			SyncStream ss = new SyncStream(s);

			XdrReader<Int32>.Read(ss, (val) =>
			{
				Assert.AreEqual(0x1234ABCD, val);
			}, (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			});
		}
	}
}

