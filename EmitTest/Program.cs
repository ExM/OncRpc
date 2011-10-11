using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Xdr;
using Rpc.BindingProtocols;
using System.Net.Sockets;
using System.Net;
using Rpc;
using Rpc.MessageProtocol;

namespace EmitTest
{
	class Program
	{
		static void Main(string[] args)
		{
			
			var conn = new UdpConnector(new ReadBuilder(), new WriteBuilder());



			var client = new PortMapper(conn);

			client.Dump((t) =>
			{
				pmaplist item = t.next;

				while (item != null)
				{
					Console.WriteLine("port:{0} prog:{1} prot:{2} vers:{3}",
						item.map.port, item.map.prog, item.map.prot, item.map.vers);
					item = item.next;
				}
			}, (e) => Console.WriteLine(e));

			/*
			var client = new RpcBindV4(conn);
			
			client.GetTime((t) => Console.WriteLine(t), (e) => Console.WriteLine(e));
			client.Dump((t) =>
			{
				rp__list item = t.Instance;
				
				while(item != null)
				{
					Console.WriteLine("addr:{0} netid:{1} owner:{2} prog:{3} vers:{4}",
						item.rpcb_map.r_addr, item.rpcb_map.r_netid, item.rpcb_map.r_owner, item.rpcb_map.r_prog, item.rpcb_map.r_vers);
					item = item.rpcb_next;
				}
			}, (e) => Console.WriteLine(e));
			*/


			/*
			rpc_msg msg = new rpc_msg();
			msg.xid = 123;
			msg.body = new body();
			msg.body.mtype = msg_type.CALL;
			msg.body.cbody = new call_body();
			msg.body.cbody.rpcvers = 2;
			msg.body.cbody.prog = 100000;
			msg.body.cbody.proc = 4;
			msg.body.cbody.vers = 4;
			msg.body.cbody.cred = opaque_auth.None;
			msg.body.cbody.verf = opaque_auth.None;
			
			
			ByteWriter bw = new ByteWriter();
			Writer w = new WriteBuilder().Create(bw);

			w.Write(msg);
			
			
			
			UdpClient client = new UdpClient(AddressFamily.InterNetwork);
			byte[] outBuff = bw.ToArray();
			
			
			IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 111);
			
			client.Send(outBuff, outBuff.Length, ep);
			
			
			byte[] inBuff = client.Receive(ref ep);
			
			int p = 0;
			foreach(byte b in inBuff)
			{
				if(p > 14)
				{
					Console.WriteLine("0x{0},", b.ToString("X2"));
					p = 0;
				}
				else
				{
					Console.Write("0x{0}, ", b.ToString("X2"));
					p++;
				}
			}
			
			ByteReader br = new ByteReader(inBuff);
			Reader r = new ReadBuilder().Create(br);
			
			rpc_msg response = r.Read<rpc_msg>();
			
			rp__list list = r.ReadOption<rp__list>();
			*/
		}
	}
}
