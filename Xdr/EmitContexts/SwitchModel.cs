using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Xdr.EmitContexts
{
	public class SwitchModel
	{
		public FieldDesc SwitchField {get; private set;}
		public Dictionary<object, FieldDesc> Branches {get; private set;}

		public Delegate BuildWriter(Type targetType)
		{
			ParameterExpression pWriter = Expression.Parameter(typeof(Writer));
			ParameterExpression pItem = Expression.Parameter(targetType);

			List<ParameterExpression> variables = new List<ParameterExpression>();
			List<Expression> body = new List<Expression>();

			LabelTarget exit = Expression.Label();


			List<SwitchCase> cases = new List<SwitchCase>();
			foreach (var branch in Branches)
				cases.Add(BuildWriteBranch(branch.Key, branch.Value, pWriter, pItem, exit));

			body.Add(
			Expression.Switch(
				Expression.PropertyOrField(pItem, SwitchField.MInfo.Name),
				Expression.Block(ThrowUnexpectedValue(Expression.PropertyOrField(pItem, SwitchField.MInfo.Name))),
				cases.ToArray())
			);

			body.Add(Expression.Label(exit));

			BlockExpression block = Expression.Block(variables, body);

			return Expression
				.Lambda(typeof(WriteOneDelegate<>).MakeGenericType(targetType), block, pWriter, pItem)
				.Compile();
		}

		private SwitchCase BuildWriteBranch(object key, FieldDesc fieldDesc, Expression pWriter, Expression pItem, LabelTarget exit)
		{
			List<Expression> body = new List<Expression>();
			body.Add(SwitchField.BuildWriteOne(pWriter, key));

			if (fieldDesc != null)
				body.Add(fieldDesc.BuildWrite(pWriter, pItem));

			body.Add(Expression.Return(exit));
			return Expression.SwitchCase(Expression.Block(body), Expression.Constant(key));
		}

		public Delegate BuildReader(Type targetType)
		{
			ParameterExpression pReader = Expression.Parameter(typeof(Reader));
			
			List<ParameterExpression> variables = new List<ParameterExpression>();
			List<Expression> body = new List<Expression>();

			ParameterExpression resultVar = Expression.Variable(targetType, "result");
			variables.Add(resultVar);

			BinaryExpression assign = Expression.Assign(resultVar, Expression.New(targetType));
			body.Add(assign);

			body.Add(SwitchField.BuildAssign(SwitchField.BuildReadOne(pReader), resultVar));

			LabelTarget exit = Expression.Label();

			List<SwitchCase> cases = new List<SwitchCase>();
			foreach (var branch in Branches)
				cases.Add(BuildReadBranch(branch.Key, branch.Value, resultVar, pReader, exit));

			body.Add(
			Expression.Switch(
				Expression.PropertyOrField(resultVar, SwitchField.MInfo.Name),
				Expression.Block(ThrowUnexpectedValue(Expression.PropertyOrField(resultVar, SwitchField.MInfo.Name))),
				cases.ToArray())
			);

			body.Add(Expression.Label(exit));
			body.Add(resultVar);

			BlockExpression block = Expression.Block(variables, body);

			return Expression
				.Lambda(typeof(ReadOneDelegate<>).MakeGenericType(targetType), block, pReader)
				.Compile();
		}

		private static Expression ThrowUnexpectedValue(MemberExpression value)
		{
			//throw new FormatException(string.Format("unexpected value: {0}", result.Type));

			var strExpr = Expression.Call(typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }),
				Expression.Constant("unexpected value: {0}"),
				Expression.Call(value, typeof(object).GetMethod("ToString")));
			return Expression.Throw(Expression.New(typeof(FormatException).GetConstructor(new Type[] { typeof(string) }), strExpr));
		}

		private static SwitchCase BuildReadBranch(object key, FieldDesc fieldDesc, ParameterExpression resultVar, Expression pReader, LabelTarget exit)
		{
			List<Expression> body = new List<Expression>();
			if (fieldDesc != null)
				body.Add(fieldDesc.BuildAssign(fieldDesc.BuildRead(pReader), resultVar));
					
			body.Add(Expression.Break(exit));
			return Expression.SwitchCase(Expression.Block(body), Expression.Constant(key));
		}
		
		public static SwitchModel Create(Type t)
		{
			SwitchModel model = new SwitchModel();
			model.Branches = new Dictionary<object, FieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(model, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(model, pi);
			
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
		
		private static void AppendField(SwitchModel model, MemberInfo mi)
		{
			if (mi.GetAttr<SwitchAttribute>() != null)
			{ // switch field
				if(model.SwitchField != null)
					throw new InvalidOperationException("duplicate switch attribute");
				
				model.SwitchField = new FieldDesc(mi);
				
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
					model.Branches.Add(cAttr.Value, new FieldDesc(mi));
				}
			}
		}
	}
}

