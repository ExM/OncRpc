using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Xdr.EmitContexts
{
	public class OrderModel
	{
		public List<FieldDesc> Fields {get; private set;}


		public Delegate BuildReader(Type targetType)
		{
			ParameterExpression pReader = Expression.Parameter(typeof(Reader));

			List<ParameterExpression> variables = new List<ParameterExpression>();
			List<Expression> body = new List<Expression>();

			ParameterExpression resultVar = Expression.Variable(targetType, "result");
			variables.Add(resultVar);

			BinaryExpression assign = Expression.Assign(resultVar, Expression.New(targetType));
			body.Add(assign);

			foreach (var fieldDesc in Fields)
			{
				body.Add(fieldDesc.BuildAssign(fieldDesc.BuildRead(pReader), resultVar));
			}

			body.Add(resultVar);

			BlockExpression block = Expression.Block(variables, body);

			return Expression
				.Lambda(typeof(ReadOneDelegate<>).MakeGenericType(targetType), block, pReader)
				.Compile();
		}

		public Delegate BuildWriter(Type targetType)
		{
			ParameterExpression pWriter = Expression.Parameter(typeof(Writer));
			ParameterExpression pItem = Expression.Parameter(targetType);

			List<ParameterExpression> variables = new List<ParameterExpression>();
			List<Expression> body = new List<Expression>();

			foreach (var fieldDesc in Fields)
				body.Add(fieldDesc.BuildWrite(pWriter, pItem));
			
			BlockExpression block = Expression.Block(variables, body);

			return Expression
				.Lambda(typeof(WriteOneDelegate<>).MakeGenericType(targetType), block, pWriter, pItem)
				.Compile();
		}

		public static OrderModel Create(Type t)
		{
			SortedList<uint, FieldDesc> fields = new SortedList<uint, FieldDesc>();
			
			foreach(var fi in t.GetFields().Where((fi) => fi.IsPublic && !fi.IsStatic))
				AppendField(fields, fi);

			foreach (var pi in t.GetProperties().Where((pi) => pi.CanWrite && pi.CanRead))
				AppendField(fields, pi);
		
			if(fields.Count == 0)
				return null;

			OrderModel result = new OrderModel();
			result.Fields = fields.Values.ToList();
			return result;
		}
		
		private static void AppendField(SortedList<uint, FieldDesc> fields, MemberInfo mi)
		{
			OrderAttribute fAttr = mi.GetAttr<OrderAttribute>();
			if (fAttr == null)
				return;

			if (fields.ContainsKey(fAttr.Order))
				throw new InvalidOperationException("duplicate order " + fAttr.Order);
			
			fields.Add(fAttr.Order, new FieldDesc(mi));
		}
	}
}

