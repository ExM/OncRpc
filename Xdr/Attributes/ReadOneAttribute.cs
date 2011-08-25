using System;
using System.Reflection;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class ReadOneAttribute: Attribute
	{
		public Type ContextType { get; private set; }

		public string MethodName { get; private set; }

		public ReadOneAttribute(Type contextType, string methodName = null)
		{
			ContextType = contextType;
			MethodName = methodName;
		}

		public Delegate Create(Type targetType)
		{
			Type complDelegateType = typeof(Action<>).MakeGenericType(targetType);
			MethodInfo readerMethod = null;

			if (MethodName == null)
			{
				foreach (var mi in ContextType.GetMethods(BindingFlags.Static | BindingFlags.Public))
				{
					if(!CheckParameters(mi, complDelegateType))
						continue;

					if (readerMethod != null)
						throw new InvalidOperationException(string.Format("Duplicate methods in {0} ({1} & {2})",
							ContextType.FullName, readerMethod.Name, mi.Name));

					readerMethod = mi;
				}

				if(readerMethod == null)
					throw new InvalidOperationException(string.Format("reader method not found in {0}", ContextType.FullName));
			}
			else
			{
				readerMethod = ContextType.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public);
				if(readerMethod == null)
					throw new InvalidOperationException(string.Format("reader method not found in {0}", ContextType.FullName));

				if(!CheckParameters(readerMethod, complDelegateType))
					throw new InvalidOperationException(string.Format("invalid parameters of method {0} in {1}",
							readerMethod.Name, ContextType.FullName));
			}

			return Delegate.CreateDelegate(typeof(ReadOneDelegate<>).MakeGenericType(targetType), readerMethod);
		}

		private bool CheckParameters(MethodInfo mi, Type complDelegateType)
		{
			ParameterInfo[] pars = mi.GetParameters();
			if (pars.Length != 3)
				return false;
			return
				pars[0].ParameterType == typeof(IReader) &&
				pars[1].ParameterType == complDelegateType &&
				pars[2].ParameterType == typeof(Action<Exception>);
		}

	}
}

