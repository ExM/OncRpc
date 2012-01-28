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
			ThreadPool.SetMinThreads(2, 2);

			IPEndPoint ep;
			ep = new IPEndPoint(IPAddress.Loopback, 111);
			//ep = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 62, 122 }), 111);

			//var conn = new TcpConnector(ep);
			var conn = new RpcClient(() => new TcpSession(ep));


			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken token = cts.Token;
			
			Stopwatch swReq = new Stopwatch();
			swReq.Start();

			var t = conn.PortMapper(token, true).Dump();
			//var tn = conn.PortMapper(cts.Token).Null();
			var tn = conn.RpcBindV4(token).GetTime();

			try
			{
				if(!Task.WaitAll(new Task[] { t, tn }, 3000))
					cts.Cancel(false);
			}
			catch
			{
			}

			Console.WriteLine("All req elapsed {0}", swReq.Elapsed);

			Console.WriteLine("t.Status {0}", t.Status);
			Console.WriteLine("tn.Status {0}", tn.Status);
			
			if(t.Exception != null)
				Console.WriteLine("t.Exception: {0}", t.Exception);
			if (t.Status == TaskStatus.RanToCompletion)
				foreach(var item in t.Result)
					Console.WriteLine("port:{0} prog:{1} prot:{2} vers:{3}",
					item.port, item.prog, item.prot, item.vers);

			if(tn.Exception != null)
				Console.WriteLine("tn.Exception: {0}", tn.Exception);
			if(tn.Status == TaskStatus.RanToCompletion)
				Console.WriteLine("tn.Result: {0}", tn.Result);

			Console.ReadLine();
			conn.Dispose();

			Console.ReadLine();
		}
	}
}
