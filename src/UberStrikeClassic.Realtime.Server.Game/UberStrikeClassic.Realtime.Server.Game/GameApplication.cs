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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common.IO;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game
{
    public class GameApplication : ApplicationBase
    {
        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        private bool isLocalServer = false;

        public GameLobby Lobby { get; private set; }

        public int ServerPort
        {
            get
            {
                return 5056;
            }
        }

        public int PlayerCount
        {
            get
            {
                var count = 0;
                foreach (var room in Lobby.Rooms.All.Values)
                    count += room.Actors.Count;
                return count;
            }
        }

        public static new GameApplication Instance => (GameApplication)ApplicationBase.Instance;

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new GamePeer(initRequest);
        }

        private string GetPrivateIP()
        {
            string localIP;
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }
            catch
            {
                localIP = "127.0.0.1";
            }
            return localIP;
        }

        private string GetPublicIP()
        {
            try
            {
                string url = "http://checkip.dyndns.org";
                WebRequest req = System.Net.WebRequest.Create(url);
                WebResponse resp = req.GetResponse();
                StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];

                return a4;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public string GetServerIP()
        {
            if (isLocalServer)
                return GetPrivateIP();
            else
                return GetPublicIP();
        }

        protected override void Setup()
        {
            ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");
            GlobalContext.Properties["LogFileName"] = "Log_" + this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(this.BinaryPath, "log4net.config")));

            RealtimeSerialization.Converter = new UberStrikeByteConverter();

            Lobby = new GameLobby();

            log.Error("Server has been started!");
        }

        protected override void TearDown()
        {
            try
            {
                Lobby.Dispose();

                // Environment.Exit(Environment.ExitCode);

                log.Error("GameApplication has been Disposed.");
            }
            catch(Exception ex)
            {
                log.Error("Exception caught while trying to dispose lobby: " + ex.Message);
            }
        }
    }
}
