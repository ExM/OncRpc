using System;

namespace Xdr2
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class CaseAttribute: Attribute
	{
		public readonly object Value;
		
		public CaseAttribute(object val)
		{
			Type vT = val.GetType();
			if (vT == typeof(int) || vT.IsEnum)
				Value = val;
			else
				throw new InvalidOperationException("required enum type or int");
		}
	}
}

