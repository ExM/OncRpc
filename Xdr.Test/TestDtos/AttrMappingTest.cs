using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;
using Xdr.EmitContexts;

namespace Xdr
{
	[TestFixture]
	public class AttrMappingTest
	{
		[TestFixtureTearDown]
		public void SaveDynamicAssembly()
		{
			EmitContext.SaveDynamicAssembly();
		}

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
		public void ReadStruct()
		{
			MemoryStream s = new MemoryStream();
			s.Write(0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<StructInt>((val) =>
			{
				Assert.AreEqual(0x1234ABCD, val.Field1);
				Assert.AreEqual(0xCDEF9876, val.Field2);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(8, s.Position);
		}
		
		[Test]
		public void Write()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			SimplyInt val = new SimplyInt();
			val.Field1 = 0x1234ABCD;
			val.Field2 = 0xCDEF9876;
			
			w.Write<SimplyInt>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76}, s.ToArray());
		}
		
		[Test]
		public void WriteStruct()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			StructInt val = new StructInt();
			val.Field1 = 0x1234ABCD;
			val.Field2 = 0xCDEF9876;
			
			w.Write<StructInt>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76}, s.ToArray());
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
			
			r.Read<ListItem>((val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(null, val.Field2);
				Assert.AreEqual(null, val.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(12, s.Position);
		}
		
		[Test]
		public void Write_Short()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = null;
			val.Field3 = null;
			
			w.Write<ListItem>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
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
			
			r.Read<ListItem>((val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(3, val.Field2);
				Assert.AreEqual(null, val.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(16, s.Position);
		}
		
		[Test]
		public void Write_OneItem()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = 3;
			val.Field3 = null;
			
			w.Write<ListItem>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
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
			
			r.Read<ListItem>((val) =>
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
		
		[Test]
		public void Write_TwoItem()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = null;
			val.Field3 = new ListItem();
			val.Field3.Field1 = 1;
			val.Field3.Field2 = 3;
			val.Field3.Field3 = null;
			
			w.Write<ListItem>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
		}
		
		[Test]
		public void Read_InternalFixArray()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Reader r = t.CreateReader(ss);
			
			r.Read<ListContainer>((val) =>
			{
				Assert.AreEqual(1, val.Field1[0]);
				Assert.AreEqual(2, val.Field1[1]);
				Assert.AreEqual(3, val.Field1[2]);
				Assert.AreEqual(0, val.Field2.Count);
				Assert.AreEqual("", val.Field3);
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));

			Assert.AreEqual(5*4, s.Position);
		}
		
		[Test]
		public void Write_InternalFixArray()
		{
			MemoryStream s = new MemoryStream();
			ITranslator t = Translator.Create().Build();
			
			SyncStream ss = new SyncStream(s);
			Writer w = t.CreateWriter(ss);
			
			ListContainer val = new ListContainer();
			val.Field1 = new int[3];
			val.Field1[0] = 1;
			val.Field1[1] = 2;
			val.Field1[2] = 3;
			val.Field2 = new List<uint>();
			val.Field3 = "";
			
			w.Write<ListContainer>(val,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			s.Position = 0;
			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
		}
	}
}

