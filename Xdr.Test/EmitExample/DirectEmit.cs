using System;
using NUnit.Framework;
using System.IO;
using Xdr.Example;
using Xdr.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Xdr.Test
{
	[TestFixture]
	public class TranslatorTest
	{
		[Test]
		public void ReadInt()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0xFF, 0xFF, 0xFF, 0xFF,
				0xFF, 0xFF, 0xFF, 0xFF,
				0x7F, 0xFF, 0xFF, 0xFF);
			s.Position = 0;


			SyncStream ss = new SyncStream(s);

			Examples.Translator t = new Examples.Translator();
			IReader r = t.Create(ss);

			Action<Exception> excepted = (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			};

			Action<int> completed1 = (val) =>
			{
				Assert.AreEqual(1, val);
			};

			r.Read<int>(completed1, excepted);

			r.Read<int>((val) => Assert.AreEqual(1, val), excepted);

			r.Read<object>(
				(val) => Assert.Fail("missed exeption"),
				(ex) => Assert.IsInstanceOf<NotImplementedException>(ex, "unknown error: {0}", ex));
		}
	}
}

