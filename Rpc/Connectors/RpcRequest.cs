using System;
using Rpc.Connectors;
using Xdr;
using Rpc.MessageProtocol;
using System.Threading;
using System.Net.Sockets;

namespace Rpc
{
	internal class RpcRequest<TResult>: IRpcRequest<TResult>, IReceivedHandler
	{
		private readonly AsyncUdpConnector _conn;
		private readonly uint _xid;
		
		
		private object _sync = new object();
		private bool _resolved = false;
		private Exception _outErr = null;
		private TResult _outResult = default(TResult);
		
		private Action<TResult> _resultRecs = null;
		private Action<Exception> _exceptRecs = null;
		
		public RpcRequest(AsyncUdpConnector conn, uint xid)
		{
			_conn = conn;
			_xid = xid;
		}
		
		public uint Xid
		{
			get
			{
				return _xid;
			}
		}
		
		public IRpcRequest<TResult> Complete(Action completed)
		{
			Result((r) => completed());
			Except((e) => completed());
			
			return this;
		}
		
		public IRpcRequest<TResult> Result(Action<TResult> resulted)
		{
			lock(_sync)
			{
				if(_resolved)
				{
					if(_outErr == null)
						ThreadPool.QueueUserWorkItem((o) => resulted(_outResult));
				}
				else
					_resultRecs += resulted;
			}
			
			return this;
		}
		
		public void Result(TResult res)
		{
			Action<TResult> copy;
			lock(_sync)
			{
				if(_resolved)
					return;
				
				_resolved = true;
				copy = _resultRecs;
				_outResult = res;
				_resultRecs = null;
				_exceptRecs = null;
			}
			_conn.RemoveHandler(_xid);
			copy(res);
		}
		
		public IRpcRequest<TResult> Except(Action<Exception> excepted)
		{
			lock(_sync)
			{
				if(_resolved)
				{
					if(_outErr != null)
						ThreadPool.QueueUserWorkItem((o) => excepted(_outErr));
				}
				else
					_exceptRecs += excepted;
			}
			
			return this;
		}
		
		public void Except(Exception ex)
		{
			Action<Exception> copy;
			lock(_sync)
			{
				if(_resolved)
					return;
				
				_resolved = true;
				copy = _exceptRecs;
				_outErr = new RpcException("request error", ex);
				ex = _outErr;
				_resultRecs = null;
				_exceptRecs = null;
			}
			_conn.RemoveHandler(_xid);
			copy(ex);
		}
		
		public IRpcRequest<TResult> Timeout(int timeout)
		{
			ManualResetEvent wait = new ManualResetEvent(false);
			RegisteredWaitHandle handle = null;
			handle = ThreadPool.RegisterWaitForSingleObject(wait,
				(object state, bool timedOut) =>
				{
					handle.Unregister(wait);
					if(timedOut)
						Except(new SocketException((int)SocketError.TimedOut));
				}, null, timeout, true);
			
			Complete(() => {wait.Set();});
			
			return this;
		}
		
		/*
		public void Break()
		{
			_excepted(new RpcException("request error", 
				new SocketException((int)SocketError.TimedOut)));
		}
*/
		
		public void ReadResult(MessageReader mr, Reader r, rpc_msg respMsg)
		{
			Exception resEx = null;
			TResult respArgs = default(TResult);
			
			try
			{
				resEx = Toolkit.ReplyMessageValidate(respMsg);
				if (resEx == null)
				{
					respArgs = r.Read<TResult>();
					mr.CheckEmpty();
				}
			}
			catch (Exception ex)
			{
				resEx = ex;
			}

			if (resEx == null)
				Result(respArgs);
			else
				Except(resEx);
		}
		
		internal void DatagramSended(IAsyncResult ar)
		{
			try
			{
				_conn.EndSend(ar);
			}
			catch(Exception ex)
			{
				Except(ex);
				return;
			}
			
			_conn.BeginReceive();
		}
	}
}

