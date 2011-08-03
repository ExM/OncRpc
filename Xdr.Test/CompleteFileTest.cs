using System;
using NUnit.Framework;
using System.IO;
using Xdr.Example;

namespace Xdr.Test
{
	[TestFixture]
	public class CompleteFileTest
	{
		[Test]
		public void Read()
		{
			MemoryStream s = new MemoryStream();
			SyncStream ss = new SyncStream(s);
			
			XdrReader<CompleteFile>.Read(ss, (val) =>
			{
				
			}, (ex) =>
			{
				
			});
			
			
			
		}
	}
}

