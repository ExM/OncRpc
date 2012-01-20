using System;
using System.Diagnostics;
using System.Net;
using Rpc;
using Rpc.BindingProtocols;
using System.Threading;
using System.Threading.Tasks;
using Rpc.Connectors;

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

			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken token = cts.Token;
			
			Stopwatch swReq = new Stopwatch();
			swReq.Start();

			var t = conn.PortMapper(token, true).Dump();
			var tn = conn.PortMapper(cts.Token).Null();
			//var tn = conn.RpcBindV4(token).GetTime();

			if(!Task.WaitAll(new Task[] { t, tn }, 2000))
				cts.Cancel(false);

			Console.WriteLine("All req elapsed {0}", swReq.Elapsed);

			Console.WriteLine("t.Status {0}", t.Status);
			Console.WriteLine("tn.Status {0}", tn.Status);
			
			if(t.Status == TaskStatus.RanToCompletion)
				foreach(var item in t.Result)
					Console.WriteLine("port:{0} prog:{1} prot:{2} vers:{3}",
					item.port, item.prog, item.prot, item.vers);
			
			if(tn.Status == TaskStatus.RanToCompletion)
				Console.WriteLine(tn.Result);

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
			
			Console.ReadLine();
			conn.Dispose();
		}
	}
}
