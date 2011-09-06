using System;
using NUnit.Framework;
using System.IO;
using Xdr.TestDtos;
using System.Collections.Generic;
using Xdr.Example;

namespace Xdr
{
	[TestFixture]
	/// <summary>
	/// read complete file structure
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public class CompleteFileTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			s.Write(
				0x00, 0x00, 0x00, 0x09,
				0x73, 0x69, 0x6c, 0x6c,
				0x79, 0x70, 0x72, 0x6f,
				0x67, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x04,
				0x6c, 0x69, 0x73, 0x70,
				0x00, 0x00, 0x00, 0x04,
				0x6a, 0x6f, 0x68, 0x6e,
				0x00, 0x00, 0x00, 0x06,
				0x28, 0x71, 0x75, 0x69,
				0x74, 0x29, 0x00, 0x00);
			s.Position = 0;
			
			ITranslator t = Translator.Create()
				.Map<CompleteFile>(CompleteFile.Read)
				.Map<FileType>(FileType.Read)
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IReader r = t.CreateReader(ss);

			CompleteFile result = null;

			r.Read<CompleteFile>((val) => 
			{
				result = val;
			}, (ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(12*4, s.Position);
			Assert.IsNotNull(result);

//        0      00 00 00 09     ....     -- length of filename = 9
//        4      73 69 6c 6c     sill     -- filename characters
//        8      79 70 72 6f     ypro     -- ... and more characters ...
//       12      67 00 00 00     g...     -- ... and 3 zero-bytes of fill
			Assert.AreEqual("sillyprog", result.FileName);
//       16      00 00 00 02     ....     -- filekind is EXEC = 2
			Assert.AreEqual(FileKind.Exec, result.Type.Type);
			Assert.IsNull(result.Type.Creator);
//       20      00 00 00 04     ....     -- length of interpretor = 4
//       24      6c 69 73 70     lisp     -- interpretor characters
			Assert.AreEqual("lisp", result.Type.Interpretor);
//       28      00 00 00 04     ....     -- length of owner = 4
//       32      6a 6f 68 6e     john     -- owner characters
			Assert.AreEqual("john", result.Owner);
//       36      00 00 00 06     ....     -- length of file data = 6
//       40      28 71 75 69     (qui     -- file data bytes ...
//       44      74 29 00 00     t)..     -- ... and 2 zero-bytes of fill
			Assert.IsNotNull(result.Data);
			Assert.AreEqual(6, result.Data.Length);
			Assert.AreEqual(0x28, result.Data[0]);
			Assert.AreEqual(0x29, result.Data[5]);
		}
		
		[Test]
		public void Write()
		{
			CompleteFile cf = new CompleteFile();
			cf.FileName = "sillyprog";
			cf.Type = new FileType();
			cf.Type.Type = FileKind.Exec;
			cf.Type.Interpretor = "lisp";
			cf.Owner = "john";
			cf.Data = new byte[] {0x28, 0x71, 0x75, 0x69, 0x74, 0x29};
			
			
			MemoryStream s = new MemoryStream();
			
			
			ITranslator t = Translator.Create()
				.Map<CompleteFile>(CompleteFile.Write)
				.Map<FileType>(FileType.Write)
				.Build();
			
			SyncStream ss = new SyncStream(s);
			IWriter w = t.CreateWriter(ss);

			w.Write<CompleteFile>(cf,
				() => {},
				(ex) => Assert.Fail("unexpected exception: {0}", ex));
			
			Assert.AreEqual(12*4, s.Position);
			
			byte[] expected = new byte[]
			{
				0x00, 0x00, 0x00, 0x09,
				0x73, 0x69, 0x6c, 0x6c,
				0x79, 0x70, 0x72, 0x6f,
				0x67, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x02,
				0x00, 0x00, 0x00, 0x04,
				0x6c, 0x69, 0x73, 0x70,
				0x00, 0x00, 0x00, 0x04,
				0x6a, 0x6f, 0x68, 0x6e,
				0x00, 0x00, 0x00, 0x06,
				0x28, 0x71, 0x75, 0x69,
				0x74, 0x29, 0x00, 0x00
			};
			
			s.Position = 0;
			Assert.AreEqual(expected, s.ToArray());
		}

	}
}

