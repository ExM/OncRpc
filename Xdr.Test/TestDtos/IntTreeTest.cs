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

		[Test]
		public void ReadForDirectEmit()
		{
			MemoryStream s = new MemoryStream();
			s.Write(new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF}, 0, 20);
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
	}
}

