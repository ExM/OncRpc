using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public static class TypeExtensions
	{

		public static Type GetKnownItemType(this Type collectionType)
		{

			if (collectionType.HasElementType)
			{ // array type
				Type elT = collectionType.GetElementType();
				if (elT != null && elT.MakeArrayType() == collectionType)
					return elT;
			}

			if (collectionType.IsGenericType)
			{
				Type genericType = collectionType.GetGenericTypeDefinition();
				if (genericType == typeof(List<>) ||
					genericType == typeof(IList<>) ||
					genericType == typeof(ICollection<>) ||
					genericType == typeof(IEnumerable<>))
					return collectionType.GetGenericArguments()[0];


			}

			return null;
		}

	}
}
