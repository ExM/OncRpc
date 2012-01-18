using System;
using System.Diagnostics;
using System.Net;
using Rpc;
using Rpc.BindingProtocols;

namespace EmitTest
{
	class Program
	{
		static void Main(string[] args)
		{

			IPEndPoint ep;
			//ep = new IPEndPoint(IPAddress.Loopback, 111);
			ep = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 62, 122 }), 111);

			var conn = new UdpConnector(ep);
			
			Stopwatch swReq = new Stopwatch();
			swReq.Start();

			var t = conn.PortMapper().Dump();
			var tn = conn.PortMapper().Null();

			t.Wait();

			Console.WriteLine("All req elapsed {0}", swReq.Elapsed);

			foreach (var item in t.Result)
				Console.WriteLine("port:{0} prog:{1} prot:{2} vers:{3}",
					item.port, item.prog, item.prot, item.vers);

			
			/*
			List<IRpcRequest<List<mapping>>> tickets = new List<IRpcRequest<List<mapping>>>();
			
			for(int i = 0; i<20; i++)
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();
				int n = i;
				
				IRpcRequest<List<mapping>> ticket = client.Dump((t) =>
				{
					Console.WriteLine("Req {0} elapsed {1} ok", n, sw.Elapsed);
					//foreach(var m in t)
					//	Console.WriteLine("port:{0} prog:{1} prot:{2} vers:{3}", m.port, m.prog, m.prot, m.vers);
				}, (e) => Console.WriteLine("Req {0} elapsed {1} err {2}", n, sw.Elapsed, e));
				
				ticket.Timeout(2000);
				
				tickets.Add(ticket);
			}
			*/
			
			tn.Wait();

			Console.WriteLine(tn.Result);

			Console.ReadLine();
			conn.Dispose();
		}
	}
}
