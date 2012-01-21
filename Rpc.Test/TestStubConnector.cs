using System;
using Rpc;
using Rpc.MessageProtocol;
using Xdr;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using NLog;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Rpc
{
	public class TestStubConnector: IConnector
	{
		private byte[] _expectedSendArray;
		private byte[] _receivedArray;

		public TestStubConnector(byte[] expectedSendArray, byte[] receivedArray)
		{
			_expectedSendArray = expectedSendArray;
			_receivedArray = receivedArray;
		}

		public static IConnector FromLog(string sendDump, string receiveDump)
		{
			return new TestStubConnector(sendDump.LogToArray(), receiveDump.LogToArray());
		}

		public Task<TResp> CreateTask<TReq, TResp>(call_body callBody, TReq reqArgs, TaskCreationOptions options, CancellationToken token)
		{
			return Task.Factory.StartNew<TResp>(() => Request<TReq, TResp>(callBody, reqArgs), token, options, TaskScheduler.Current);
		}

		public TResp Request<TReq, TResp>(call_body callBody, TReq reqArgs)
		{
			MessageReader mr = new MessageReader(_receivedArray);
			Reader r = Toolkit.CreateReader(mr);
			rpc_msg respMsg = r.Read<rpc_msg>();

			
			rpc_msg reqHeader = new rpc_msg()
			{
				xid = respMsg.xid, // use xid in _receivedArray
				body = new body()
				{
					mtype = msg_type.CALL,
					cbody = callBody
				}
			};

			UdpDatagram dtg = new UdpDatagram();
			Writer w = Toolkit.CreateWriter(dtg);
			w.Write(reqHeader);
			w.Write(reqArgs);

			byte[] outBuff = dtg.ToArray();
			Assert.AreEqual(_expectedSendArray, outBuff, "send dump is difference");

			Toolkit.ReplyMessageValidate(respMsg);
			TResp respArgs = r.Read<TResp>();
			mr.CheckEmpty();

			return respArgs;
		}
	}
}

