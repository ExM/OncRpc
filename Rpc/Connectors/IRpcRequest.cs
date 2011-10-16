using System;

namespace Rpc
{
	public interface IRpcRequest<TResult>
	{
		IRpcRequest<TResult> Complete(Action completed);
		
		IRpcRequest<TResult> Result(Action<TResult> resulted);
		
		void Result(TResult res);
		
		IRpcRequest<TResult> Except(Action<Exception> excepted);
		
		void Except(Exception ex);
	}
}

