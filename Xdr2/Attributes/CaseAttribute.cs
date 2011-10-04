using System;

namespace Xdr2
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class CaseAttribute: Attribute
	{
		public readonly int Value;
		
		public CaseAttribute(object val)
		{
			Type vT = val.GetType();
			if(vT == typeof(int))
			{
				Value = (int)val;
				return;
			}
			
			if(!vT.IsEnum)
				throw new InvalidOperationException("required enum type or int");
			
			Type underType = vT.GetEnumUnderlyingType();
			if(underType == typeof(byte))
				Value = (int)(byte)(ValueType)val;
			else if(underType == typeof(sbyte))
				Value = (int)(sbyte)(ValueType)val;
			else if(underType == typeof(short))
				Value = (int)(short)(ValueType)val;
			else if(underType == typeof(ushort))
				Value = (int)(ushort)(ValueType)val;
			else if(underType == typeof(int))
				Value = (int)(ValueType)val;
			else
				throw new NotSupportedException(string.Format("unsupported type {0}", vT.FullName));
		}
	}
}

