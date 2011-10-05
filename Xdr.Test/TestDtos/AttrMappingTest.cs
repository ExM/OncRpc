using System;
using NUnit.Framework;
using System.IO;
using Xdr2.TestDtos;
using System.Collections.Generic;

namespace Xdr2
{
	[TestFixture]
	public class AttrMappingTest
	{
		[Test]
		public void Read()
		{
			ByteReader s = new ByteReader(
				0x12, 0x34, 0xAB, 0xCD,
				0xCD, 0xEF, 0x98, 0x76);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);
			
			var val = r.Read<SimplyInt>();
			Assert.AreEqual(0x1234ABCD, val.Field1);
			Assert.AreEqual(0xCDEF9876, val.Field2);
			Assert.AreEqual(8, s.Position);
		}
		
		[Test]
		public void ReadStruct()
		{
			ByteReader s = new ByteReader(
				0x12, 0x34, 0xAB, 0xCD,
				0xCD, 0xEF, 0x98, 0x76);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);
			
			var val = r.Read<StructInt>();
			Assert.AreEqual(0x1234ABCD, val.Field1);
			Assert.AreEqual(0xCDEF9876, val.Field2);
			Assert.AreEqual(8, s.Position);
		}
		
		[Test]
		public void Write()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			SimplyInt val = new SimplyInt();
			val.Field1 = 0x1234ABCD;
			val.Field2 = 0xCDEF9876;
			
			w.Write<SimplyInt>(val);

			Assert.AreEqual(new byte[]{0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76}, s.ToArray());
		}
		
		[Test]
		public void WriteStruct()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			StructInt val = new StructInt();
			val.Field1 = 0x1234ABCD;
			val.Field2 = 0xCDEF9876;
			
			w.Write<StructInt>(val);

			Assert.AreEqual(new byte[]{0x12, 0x34, 0xAB, 0xCD, 0xCD, 0xEF, 0x98, 0x76}, s.ToArray());
		}
		
		[Test]
		public void Read_Short()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);
			
			var val = r.Read<ListItem>();
			Assert.AreEqual(1, val.Field1);
			Assert.AreEqual(null, val.Field2);
			Assert.AreEqual(null, val.Field3);
			Assert.AreEqual(12, s.Position);
		}
		
		[Test]
		public void Write_Short()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = null;
			val.Field3 = null;
			
			w.Write<ListItem>(val);

			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
		}
		
		[Test]
		public void Read_OneItem()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.Read<ListItem>();
			Assert.AreEqual(1, val.Field1);
			Assert.AreEqual(3, val.Field2);
			Assert.AreEqual(null, val.Field3);
			Assert.AreEqual(16, s.Position);
		}
		
		[Test]
		public void Write_OneItem()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = 3;
			val.Field3 = null;
			
			w.Write<ListItem>(val);

			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
		}
		
		[Test]
		public void Read_TwoItem()
		{
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.Read<ListItem>();
			Assert.AreEqual(1, val.Field1);
			Assert.AreEqual(null, val.Field2);
			Assert.IsNotNull(val.Field3);
				
			Assert.AreEqual(1, val.Field3.Field1);
			Assert.AreEqual(3, val.Field3.Field2);
			Assert.AreEqual(null, val.Field3.Field3);
			Assert.AreEqual(7*4, s.Position);
		}
		
		[Test]
		public void Write_TwoItem()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			ListItem val = new ListItem();
			val.Field1 = 1;
			val.Field2 = null;
			val.Field3 = new ListItem();
			val.Field3.Field1 = 1;
			val.Field3.Field2 = 3;
			val.Field3.Field3 = null;
			
			w.Write<ListItem>(val);

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
			ByteReader s = new ByteReader(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00);

			ReadBuilder builder = new ReadBuilder();
			Reader r = builder.Create(s);

			var val = r.Read<ListContainer>();
			Assert.AreEqual(1, val.Field1[0]);
			Assert.AreEqual(2, val.Field1[1]);
			Assert.AreEqual(3, val.Field1[2]);
			Assert.AreEqual(0, val.Field2.Count);
			Assert.AreEqual("", val.Field3);
			Assert.AreEqual(5*4, s.Position);
		}
		
		[Test]
		public void Write_InternalFixArray()
		{
			ByteWriter s = new ByteWriter();

			WriteBuilder b = new WriteBuilder();
			Writer w = b.Create(s);
			
			ListContainer val = new ListContainer();
			val.Field1 = new int[3];
			val.Field1[0] = 1;
			val.Field1[1] = 2;
			val.Field1[2] = 3;
			val.Field2 = new List<uint>();
			val.Field3 = "";
			
			w.Write<ListContainer>(val);
			
			Assert.AreEqual(new byte[]{
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x03,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00}, s.ToArray());
		}
	}
}

