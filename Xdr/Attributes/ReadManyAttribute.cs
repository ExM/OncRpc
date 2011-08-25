using System;
using System.Reflection;

namespace Xdr
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class ReadManyAttribute: Attribute
	{
		public Type ContextType { get; private set; }

		public string[] MethodNames { get; private set; }

		public ReadManyAttribute(Type contextType, params string[] methodName)
		{
			ContextType = contextType;
			if (methodName == null)
				methodName = new string[0];
			MethodNames = methodName;
		}

		public Delegate Create(Type targetType)
		{
			Type complDelegateType = typeof(Action<>).MakeGenericType(targetType);
			MethodInfo readerMethod = null;

			if (MethodNames.Length == 0)
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
			}
			else
			{
				foreach (string mName in MethodNames)
				{
					MethodInfo mi = ContextType.GetMethod(mName, BindingFlags.Static | BindingFlags.Public);

					if (mi == null)
						continue;

					if (!CheckParameters(mi, complDelegateType))
						continue;

					if (readerMethod != null)
						throw new InvalidOperationException(string.Format("Duplicate methods in {0} ({1} & {2})",
							ContextType.FullName, readerMethod.Name, mi.Name));

					readerMethod = mi;
				}
			}

			if (readerMethod == null)
				return null;

			return Delegate.CreateDelegate(typeof(ReadManyDelegate<>).MakeGenericType(targetType), readerMethod);
		}

		private bool CheckParameters(MethodInfo mi, Type complDelegateType)
		{
			ParameterInfo[] pars = mi.GetParameters();
			if (pars.Length != 5)
				return false;
			return
				pars[0].ParameterType == typeof(IReader) &&
				pars[1].ParameterType == typeof(uint) &&
				pars[2].ParameterType == typeof(bool) &&
				pars[3].ParameterType == complDelegateType &&
				pars[4].ParameterType == typeof(Action<Exception>);
		}

	}
}

