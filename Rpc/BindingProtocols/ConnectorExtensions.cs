using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rpc.BindingProtocols.TaskBuilders;
using System.Threading;

namespace Rpc.BindingProtocols
{
	/// <summary>
	/// Connector extensions.
	/// </summary>
	public static class ConnectorExtensions
	{
		/// <summary>
		/// The portmapper program currently supports two protocols (UDP and TCP).  The portmapper is contacted by talking to it on assigned port
		/// number 111 (SUNRPC) on either of these protocols.
		/// http://tools.ietf.org/html/rfc1833#section-3.2
		/// </summary>
		public static PortMapper PortMapper(this IConnector conn, CancellationToken token, bool attachedToParent = false)
		{
			return new PortMapper(conn, token, attachedToParent);
		}

		/// <summary>
		/// The portmapper program currently supports two protocols (UDP and TCP).  The portmapper is contacted by talking to it on assigned port
		/// number 111 (SUNRPC) on either of these protocols.
		/// http://tools.ietf.org/html/rfc1833#section-3.2
		/// </summary>
		public static PortMapper PortMapper(this IConnector conn, bool attachedToParent = false)
		{
			return new PortMapper(conn, CancellationToken.None, attachedToParent);
		}

		/// <summary>
		/// RPCBIND Version 3
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		public static RpcBindV3 RpcBindV3(this IConnector conn, CancellationToken token, bool attachedToParent = false)
		{
			return new RpcBindV3(conn, token, attachedToParent);
		}

		/// <summary>
		/// RPCBIND Version 3
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		public static RpcBindV3 RpcBindV3(this IConnector conn, bool attachedToParent = false)
		{
			return new RpcBindV3(conn, CancellationToken.None, attachedToParent);
		}

		/// <summary>
		/// RPCBIND Version 4
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		public static RpcBindV4 RpcBindV4(this IConnector conn, CancellationToken token, bool attachedToParent = false)
		{
			return new RpcBindV4(conn, token, attachedToParent);
		}

		/// <summary>
		/// RPCBIND Version 4
		/// 
		/// RPCBIND is contacted by way of an assigned address specific to the transport being used.
		/// For TCP/IP and UDP/IP, for example, it is port number 111. Each transport has such an assigned, well-known address.
		/// http://tools.ietf.org/html/rfc1833#section-2.2.1
		/// </summary>
		public static RpcBindV4 RpcBindV4(this IConnector conn, bool attachedToParent = false)
		{
			return new RpcBindV4(conn, CancellationToken.None, attachedToParent);
		}
	}
}
