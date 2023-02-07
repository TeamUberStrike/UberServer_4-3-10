using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Comm
{
    public enum LobbyOperationRequestCode : byte 
    {
        Server = 82,
        Player = 80,
        Application = 66,
        JoinRoom = 88,
        LeaveRoom = 83
    }
}
