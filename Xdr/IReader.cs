using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xdr
{
	public interface IReader
	{
		void Read<T>(Action<T> completed, Action<Exception> excepted) where T: new();
		void Read<T>(uint len, bool fix, Action<T[]> completed, Action<Exception> excepted) where T : new();
		void Read<C, T>(uint len, bool fix, Action<C> completed, Action<Exception> excepted) where T : new() where C: ICollection<T>, new();
	}
}
