using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;

namespace Xdr
{
	[TestFixture]
	public class AttrMappingTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<SimplyInt>((val) =>
			{
				Assert.AreEqual(0x1234ABCD, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(8, s.Position);
		}
		
		[Test]
		public void Read_Short()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<AllVariantOrder>((val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(null, val.Field2);
				Assert.AreEqual(null, val.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(12, s.Position);
		}
		
		[Test]
		public void Read_OneItem()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<AllVariantOrder>((val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(3, val.Field2);
				Assert.AreEqual(null, val.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(16, s.Position);
		}
		
		[Test]
		public void Read_TwoItem()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<AllVariantOrder>((val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(null, val.Field2);
				Assert.IsNotNull(val.Field3);
				
				Assert.AreEqual(1, val.Field3.Field1);
				Assert.AreEqual(3, val.Field3.Field2);
				Assert.AreEqual(null, val.Field3.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(7*4, s.Position);
		}
	}
}

