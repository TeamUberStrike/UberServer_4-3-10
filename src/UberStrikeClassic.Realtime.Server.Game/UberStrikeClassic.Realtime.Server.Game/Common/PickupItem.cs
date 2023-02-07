using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberStrikeClassic.Realtime.Server.Game.Common
{
    public class PickupItem
    {
        public int Id { get; set; }

        public TimeSpan RespawnTime { get; set; }
    }
}
