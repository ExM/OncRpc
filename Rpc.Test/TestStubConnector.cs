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
			return new TestStubConnector(ByteArrayInLog(sendDump), ByteArrayInLog(receiveDump));
		}

		public static byte[] ByteArrayInLog(string text)
		{
			MemoryStream mem = new MemoryStream();
			int hDig = -1;

			for (int i = 0; i < text.Length; i++)
			{
				int num = ParseDigit(text[i]);
				if (num == -1)
					continue;

				if (hDig == -1)
				{
					hDig = num;
					continue;
				}

				mem.WriteByte((byte)(hDig * 16 + num));
				hDig = -1;
			}

			mem.Position = 0;
			return mem.ToArray();
		}

		public static int ParseDigit(Char ch)
		{
			switch (Char.ToLowerInvariant(ch))
			{
				case '0': return 0;
				case '1': return 1;
				case '2': return 2;
				case '3': return 3;
				case '4': return 4;
				case '5': return 5;
				case '6': return 6;
				case '7': return 7;
				case '8': return 8;
				case '9': return 9;
				case 'a': return 10;
				case 'b': return 11;
				case 'c': return 12;
				case 'd': return 13;
				case 'e': return 14;
				case 'f': return 15;
				default: return -1;
			}
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

