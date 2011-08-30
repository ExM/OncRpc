using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;
using Xdr.Test.TestDtos;

namespace Xdr
{
	[TestFixture]
	public class EnumTest
	{
		[Test]
		public void ExceptionRead()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create("test").Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			r.Read<IntEnum>((val) =>
			{
				Assert.Fail("missed exception");
			}, (ex) => Assert.IsInstanceOf<InvalidCastException>(ex));

			Assert.AreEqual(4, s.Position);
		}

		[Test]
		public void ReadOne()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x01);
			s.Position = 0;

			ITranslator t = Translator.Create("test").Build();

			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			r.Read<IntEnum>((val) =>
			{
				Assert.AreEqual(IntEnum.One, val);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(4, s.Position);
		}

		[Test]
		public void ReadTwo()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x02);
			s.Position = 0;

			ITranslator t = Translator.Create("test").Build();

			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			r.Read<IntEnum>((val) =>
			{
				Assert.AreEqual(IntEnum.Two, val);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(4, s.Position);
		}
		
		[Test]
		public void ReadByteOne()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x00, 0x00, 0x00, 0x01);
			s.Position = 0;

			ITranslator t = Translator.Create("test").Build();

			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			r.Read<ByteEnum>((val) =>
			{
				Assert.AreEqual(ByteEnum.One, val);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(4, s.Position);
		}
	}
}

