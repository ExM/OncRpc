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
	public class DirectEmit
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

			AssemblyName asmName = new AssemblyName("TestDynamicAssembly");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);

			CustomContextBuilder ccb = new CustomContextBuilder(modBuilder, (t) => { throw new NotImplementedException(); });

			Type tContext = ccb.Build(typeof(IntTree));

			Action<IntTree> completed = (val) =>
			{
				Assert.AreEqual(1, val.Field1);
				Assert.AreEqual(1, val.Field2);
				Assert.AreEqual(-1, val.Field3);
				Assert.AreEqual(uint.MaxValue, val.Field4);
				Assert.AreEqual(int.MaxValue, val.Field5);
			};

			Action<Exception> excepted = (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			};

			Activator.CreateInstance(tContext, (IByteReader)ss, completed, excepted);
		}
		
		[Test]
		public void ReadString()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x01,
				(byte)'Q', 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x04,
				(byte)'H', (byte)'e', (byte)'l', (byte)'l',
				0xFF, 0xFF, 0xFF, 0xFF);
			s.Position = 0;

			SyncStream ss = new SyncStream(s);

			AssemblyName asmName = new AssemblyName("TestDynamicAssembly");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);

			CustomContextBuilder ccb = new CustomContextBuilder(modBuilder, (t) => { throw new NotImplementedException(); });

			Type tContext = ccb.Build(typeof(StringContainer));

			Action<StringContainer> completed = (val) =>
			{
				Assert.AreEqual(1, val.FI0);
				Assert.AreEqual("Q", val.FStr1);
				Assert.AreEqual("Hell", val.FStr2);
				Assert.AreEqual(-1, val.FI3);
			};

			Action<Exception> excepted = (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			};

			Activator.CreateInstance(tContext, (IByteReader)ss, completed, excepted);
		}
		
		[Test]
		public void ReadEmptyString()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0xFF, 0xFF, 0xFF, 0xFF);
			s.Position = 0;

			SyncStream ss = new SyncStream(s);

			AssemblyName asmName = new AssemblyName("TestDynamicAssembly");
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(asmName.Name);

			CustomContextBuilder ccb = new CustomContextBuilder(modBuilder, (t) => { throw new NotImplementedException(); });

			Type tContext = ccb.Build(typeof(StringContainer));

			Action<StringContainer> completed = (val) =>
			{
				Assert.AreEqual(1, val.FI0);
				Assert.AreEqual("", val.FStr1);
				Assert.AreEqual("", val.FStr2);
				Assert.AreEqual(-1, val.FI3);
			};

			Action<Exception> excepted = (ex) =>
			{
				Assert.Fail("unexpected exception: {0}", ex);
			};

			Activator.CreateInstance(tContext, (IByteReader)ss, completed, excepted);
		}
	}
}

