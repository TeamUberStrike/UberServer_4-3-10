using Cmune.Realtime.Common;
using Cmune.Realtime.Common.IO;
using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.Events
{
    public abstract class BaseEvents
    {
        protected GamePeer Peer { get; }

        private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();

        protected BaseEvents(GamePeer peer)
        {
            Peer = peer ?? throw new ArgumentNullException(nameof(peer));

            log.ErrorFormat("Fucking test with BaseEvents, Peer: {0}", peer.ConnectionId);
        }

        protected SendResult SendEvent(byte eventCode, short networkId, byte methodId, object[] data, bool unreliable = false)
        {
            var parameters = OperationFactory.Create(CmuneOperationCodes.MessageToServer, new object[]
            {
                networkId,
                methodId,
                RealtimeSerialization.ToBytes(data).ToArray()
            });
            var eventData = new EventData(eventCode, parameters);

            return Peer.SendEvent(eventData, new SendParameters { Unreliable = unreliable });
        }

        protected SendResult SendEvent(byte eventCode, short networkId, byte methodId, byte[] data, bool unreliable = false)
        {
            var parameters = OperationFactory.Create(CmuneOperationCodes.MessageToServer, new object[]
            {
                networkId,
                methodId,
                data
            });
            var eventData = new EventData(eventCode, parameters);

            return Peer.SendEvent(eventData, new SendParameters { Unreliable = unreliable });
        }

        protected SendResult SendEvent(short networkId, byte methodId, object[] data, bool unreliable = false) => SendEvent(0, networkId, methodId, data, unreliable);

        protected SendResult SendEvent(short networkId, byte methodId, byte[] data, bool unreliable = false) => SendEvent(0, networkId, methodId, data, unreliable);

        // protected SendResult SendEvent(byte eventCode, short networkId, byte methodId, object[] data, bool unrealiable=false) => SendEvent(eventCode, networkId, methodId, data, unreliable);

        protected SendResult SendEvent(byte eventCode, Dictionary<byte, object> parameters, bool unreliable = false)
        {
            var eventData = new EventData(eventCode, parameters);

            return Peer.SendEvent(eventData, new SendParameters { Unreliable = unreliable });
        }
    }
}
