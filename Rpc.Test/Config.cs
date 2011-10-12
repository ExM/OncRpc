using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Net;

namespace Rpc
{
	public static class Config
	{
		static Config()
		{
			LoggingConfiguration config = new LoggingConfiguration();
			ConsoleTarget consoleTarget = new ConsoleTarget();
			config.AddTarget("console", consoleTarget);
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss.fff}, ${logger}, ${level}, Th:${threadid}, ${message}";
			LoggingRule rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
			config.LoggingRules.Add(rule1);
			LogManager.Configuration = config;

			PortMapperAddr = new IPEndPoint(IPAddress.Loopback, 111);
		}

		public readonly static IPEndPoint PortMapperAddr;
	}

}
