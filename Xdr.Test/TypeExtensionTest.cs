using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Xdr.Test
{
	[TestFixture]
	public class TypeExtensionTest
	{
		[TestCase(typeof(List<int>), typeof(int))]
		[TestCase(typeof(IList<int>), typeof(int))]
		[TestCase(typeof(int[]), typeof(int))]
		[TestCase(typeof(List<string>), typeof(string))]
		[TestCase(typeof(string[]), typeof(string))]
		[TestCase(typeof(int), null)]
		[TestCase(typeof(int?), null)]
		[TestCase(typeof(TypeExtensionTest), null)]
		[TestCase(typeof(Tuple<int, string>), null)]
		public void GetItemType(Type coll, Type item)
		{
			Assert.AreEqual(item, coll.GetKnownItemType());
		}
	}
}

