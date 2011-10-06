using System;
using Xdr;

namespace Rpc.BindingProtocols
{
	
	public class rp__list
	{
		[Order(0)]
		public rpcb rpcb_map;
		
		[Order(1), Option]
		public rp__list rpcb_next;
	};
	
	/// <summary>
	/// results of RPCBPROC_DUMP
	/// </summary>
	public class rpcblist_ptr
	{
		[Order(0), Option]
		public rp__list rpcb_next;
	}

	/// <summary>
	/// Arguments of remote calls
	/// </summary>
	public class rpcb_rmtcallargs
	{
		/// <summary>
		/// program number
		/// </summary>
		[Order(0)]
		public ulong prog;
		/// <summary>
		/// version number
		/// </summary>
		[Order(1)]
		public ulong vers;
		/// <summary>
		/// procedure number
		/// </summary>
		[Order(2)]
		public ulong proc;
		/// <summary>
		/// argument
		/// </summary>
		[Order(3), Var]
		public byte[] args;
	};

	/// <summary>
	/// Results of the remote call
	/// </summary>
	public class rpcb_rmtcallres
	{
		/// <summary>
		/// remote universal address
		/// </summary>
		[Order(0), Var]
		public string addr;
		/// <summary>
		/// result
		/// </summary>
		[Order(1), Var]
		public byte[] results;
	};


/*
 * rpcb_entry contains a merged address of a service on a particular
 * transport, plus associated netconfig information.  A list of
 * rpcb_entry items is returned by RPCBPROC_GETADDRLIST.  The meanings
 * and values used for the r_nc_* fields are given below.
 *
 * The network identifier  (r_nc_netid):

 *   This is a string that represents a local identification for a
 *   network.  This is defined by a system administrator based on
 *   local conventions, and cannot be depended on to have the same
 *   value on every system.
 *
 * Transport semantics (r_nc_semantics):
 *  This represents the type of transport, and has the following values:
 *     NC_TPI_CLTS     (1)      Connectionless
 *     NC_TPI_COTS     (2)      Connection oriented
 *     NC_TPI_COTS_ORD (3)      Connection oriented with graceful close
 *     NC_TPI_RAW      (4)      Raw transport
 *
 * Protocol family (r_nc_protofmly):
 *   This identifies the family to which the protocol belongs.  The
 *   following values are defined:
 *     NC_NOPROTOFMLY   "-"
 *     NC_LOOPBACK      "loopback"
 *     NC_INET          "inet"
 *     NC_IMPLINK       "implink"
 *     NC_PUP           "pup"
 *     NC_CHAOS         "chaos"
 *     NC_NS            "ns"
 *     NC_NBS           "nbs"
 *     NC_ECMA          "ecma"
 *     NC_DATAKIT       "datakit"
 *     NC_CCITT         "ccitt"
 *     NC_SNA           "sna"
 *     NC_DECNET        "decnet"
 *     NC_DLI           "dli"
 *     NC_LAT           "lat"
 *     NC_HYLINK        "hylink"
 *     NC_APPLETALK     "appletalk"
 *     NC_NIT           "nit"
 *     NC_IEEE802       "ieee802"
 *     NC_OSI           "osi"
 *     NC_X25           "x25"
 *     NC_OSINET        "osinet"
 *     NC_GOSIP         "gosip"
 *
 * Protocol name (r_nc_proto):
 *   This identifies a protocol within a family.  The following are
 *   currently defined:
 *      NC_NOPROTO      "-"
 *      NC_TCP          "tcp"
 *      NC_UDP          "udp"
 *      NC_ICMP         "icmp"
 */
	/// <summary>
	/// 
	/// </summary>
	public class rpcb_entry
	{
		/// <summary>
		/// merged address of service
		/// </summary>
		[Order(0), Var]
		public string r_maddr;
		/// <summary>
		/// netid field
		/// </summary>
		[Order(1), Var]
		public string r_nc_netid;
		/// <summary>
		/// semantics of transport
		/// </summary>
		[Order(2)]
		public ulong r_nc_semantics;
		/// <summary>
		/// protocol family
		/// </summary>
		[Order(3), Var]
		public string r_nc_protofmly;
		/// <summary>
		/// protocol name
		/// </summary>
		[Order(4), Var]
		public string r_nc_proto;
	};

	/// <summary>
	/// A list of addresses supported by a service.
	/// </summary>
	public class rpcb_entry_list
	{
		[Order(0)]
		public rpcb_entry rpcb_entry_map;
		[Order(0), Option]
		public rpcb_entry_list rpcb_entry_next;
	};

	public class rpcb_entry_list_ptr
	{
		[Order(0), Option]
		public rpcb_entry_list rpcb_entry_next;
	}
/*
 * rpcbind statistics
 */

	/// <summary>
	/// Link list of all the stats about getport and getaddr
	/// </summary>
	public class rpcbs_addrlist
	{
		[Order(0)]
		public ulong prog;
		[Order(1)]
		public ulong vers;
		[Order(2)]
		public int success;
		[Order(3)]
		public int failure;
		[Order(4), Var]
		public string netid;
		[Order(5), Option]
		public rpcbs_addrlist next;
	};

	/// <summary>
	/// Link list of all the stats about rmtcall
	/// </summary>
	public class rpcbs_rmtcalllist
	{
		[Order(0)]
		public ulong prog;
		[Order(1)]
		public ulong vers;
		[Order(2)]
		public ulong proc;
		[Order(3)]
		public int success;
		[Order(4)]
		public int failure;
		[Order(5)]
		public int indirect;    /* whether callit or indirect */
		[Order(6), Var]
		public string netid;
		[Order(7), Option]
		rpcbs_rmtcalllist next;
	};

	public class rpcb_stat
	{
		[Order(0), Fix(BindingProtocol.RPCBSTAT_HIGHPROC)]
		public int[] info;
		[Order(1)]
		public int setinfo;
		[Order(2)]
		public int unsetinfo;
		[Order(3), Option]
		public rpcbs_addrlist addrinfo;
		[Order(4), Option]
		public rpcbs_rmtcalllist rmtinfo;
	};

/*
 * One rpcb_stat structure is returned for each version of rpcbind
 * being monitored.
 */

//typedef rpcb_stat rpcb_stat_byvers[RPCBVERS_STAT];

	/// <summary>
	/// netbuf structure, used to store the transport specific form of a universal transport address.
	/// </summary>
	public class netbuf
	{
		[Order(0)]
		public uint maxlen;
		[Order(1), Var]
		public byte[] buf;
	};
}
