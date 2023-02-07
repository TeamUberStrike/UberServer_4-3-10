using Cmune.DataCenter.Common.Entities;
using Cmune.Realtime.Common.IO;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net;
using log4net.Config;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common.IO;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public class CommServerApplication : ApplicationBase
    {
        private static readonly ILogger Logging = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        public static new CommServerApplication Instance => (CommServerApplication)ApplicationBase.Instance;

        public LobbyRoom Room { get; private set; }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new CommPeer(initRequest);
        }

        protected override void Setup()
        {
            Room = new LobbyRoom();

            RealtimeSerialization.Converter = new UberStrikeByteConverter();

            ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");
            GlobalContext.Properties["LogFileName"] = "MS" + this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(this.BinaryPath, "log4net.config")));

            if (Logging.IsDebugEnabled) 
            {
                Logging.Debug(">------------- Comm Server started successfully -------------<");
            }
        }

        protected override void TearDown()
        {
           
        }
    }
}
