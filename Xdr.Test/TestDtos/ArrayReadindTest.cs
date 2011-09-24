using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class ArrayReadingTest
	{
		[Test]
		public void ReadFixList()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.ReadFix<List<int>>(3, (val) =>
			{
				Assert.AreEqual(3, val.Count);
				Assert.AreEqual(1, val[0]);
				Assert.AreEqual(2, val[1]);
				Assert.AreEqual(3, val[2]);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(12, s.Position);
		}

		[Test]
		public void ReadVarList()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.ReadVar<List<int>>(4, (val) =>
			{
				Assert.AreEqual(3, val.Count);
				Assert.AreEqual(2, val[0]);
				Assert.AreEqual(3, val[1]);
				Assert.AreEqual(4, val[2]);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(16, s.Position);
		}
		
		[Test]
		public void ReadFixArray()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.ReadFix<int[]>(3, (val) =>
			{
				Assert.AreEqual(3, val.Length);
				Assert.AreEqual(1, val[0]);
				Assert.AreEqual(2, val[1]);
				Assert.AreEqual(3, val[2]);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(12, s.Position);
		}
		
		[Test]
		public void ReadVarArray()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x04);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.ReadVar<int[]>(4, (val) =>
			{
				Assert.AreEqual(3, val.Length);
				Assert.AreEqual(2, val[0]);
				Assert.AreEqual(3, val[1]);
				Assert.AreEqual(4, val[2]);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(16, s.Position);
		}
	}
}

