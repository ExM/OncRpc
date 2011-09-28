using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xdr.EmitContexts
{
	public class SwitchModel
	{
		public FieldDesc SwitchField {get; private set;}
		public Dictionary<int, FieldDesc> Branches {get; private set;}
		
		public Type BuildReadContext(ModuleBuilder mb, Type targetType)
		{
			SwitchReadContextBuilder builder = new SwitchReadContextBuilder(mb, targetType, this);
			return builder.Build();
		}

		public Type BuildWriteContext(ModuleBuilder mb, Type targetType)
		{
			SwitchWriteContextBuilder builder = new SwitchWriteContextBuilder(mb, targetType, this);
			return builder.Build();
		}
		
		public static SwitchModel Create(Type t)
		{
			SwitchModel model = new SwitchModel();
			model.Branches = new Dictionary<int, FieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(model, fi.FieldType, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(model, pi.PropertyType, pi);
			
			if(model.SwitchField == null && model.Branches.Count == 0)
				return null;
			
			if(model.SwitchField == null)
				throw new InvalidOperationException("switch attribute not found");
			
			if(model.Branches.Count <= 1)
				throw new InvalidOperationException("requires more than two case attributes");
			
			if(model.Branches.Values.All((f) => f == null))
				throw new InvalidOperationException("required no void case attribute");
			
			return model;
		}
		
		private static void AppendField(SwitchModel model, Type fieldType, MemberInfo mi)
		{
			if (mi.GetAttr<SwitchAttribute>() != null)
			{ // switch field
				if(model.SwitchField != null)
					throw new InvalidOperationException("duplicate switch attribute");
				
				model.SwitchField = FieldDesc.Create(fieldType, mi);
				
				foreach(var cAttr in mi.GetAttrs<CaseAttribute>())
				{
					if(model.Branches.ContainsKey(cAttr.Value))
						throw new InvalidOperationException("duplicate case value " + cAttr.Value.ToString());
					model.Branches.Add(cAttr.Value, null);
				}
			}
			else
			{ // case field
				foreach(var cAttr in mi.GetAttrs<CaseAttribute>())
				{
					if(model.Branches.ContainsKey(cAttr.Value))
						throw new InvalidOperationException("duplicate case value " + cAttr.Value.ToString());
					model.Branches.Add(cAttr.Value, FieldDesc.Create(fieldType, mi));
				}
			}
		}
	}
}

