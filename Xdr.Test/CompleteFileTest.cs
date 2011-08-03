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
			s.XdrRead<CompleteFile>((val) =>
			{
				
			}, (ex) =>
			{
				
			});
			
			
			
		}
	}
}

