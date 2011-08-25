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

			ITranslator t = Translator.Create("test").Build();
			IReader r = t.Create(ss);

			Action<Exception> excepted = (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			};

			r.Read<int>((val) => Assert.AreEqual(1, val), excepted);
			Assert.AreEqual(4, s.Position);
			r.Read<uint>((val) => Assert.AreEqual(1, val), excepted);
			Assert.AreEqual(8, s.Position);
			r.Read<uint>((val) => Assert.AreEqual(uint.MaxValue, val), excepted);
			Assert.AreEqual(12, s.Position);
			r.Read<byte[]>(3, true, (val) => Assert.AreEqual(3, val.Length), excepted);
			Assert.AreEqual(16, s.Position);
			r.Read<int>((val) => Assert.AreEqual(int.MaxValue, val), excepted);
			Assert.AreEqual(20, s.Position);
			
			r.Read<object>(
				(val) => Assert.Fail("missed exeption"),
				(ex) => Assert.IsInstanceOf<NotImplementedException>(ex, "unknown error: {0}", ex));
		}
	}
}

