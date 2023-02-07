using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.Network
{
    public enum INetworkClassType
    {
        GameMode,
        PeerMode
    }

    public enum HandlerType 
    {
        Player,
        Server,
        Others,
        Application
    }

    public enum PhotonGameType 
    {
        Join,
        Leave
    }
}
