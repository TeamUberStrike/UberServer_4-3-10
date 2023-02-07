using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;

namespace UberStrikeClassic.Realtime.Server.Game.Common
{
    public class GameServerConfig
    {
        public const short MaxHealth = 200;

        public const short MaxArmor = 200;

        public const ushort XpPerKill = 10;

        public const ushort XpPerCriticalKill = 16;

        public const ushort PointsPerKill = 10;

        public const int HealthArmorDecreaseOverTime = 1500;

        public const int GameFrameUpdateInterval = 100;

        public const int DefaultHealth = 100;

        public const int RespawnTime = 5;

        public const int SuicideRespawnTime = 7;

        public static ushort GetXpForKill(BodyPart bodyPart)
        {
            if (bodyPart == BodyPart.Head || bodyPart == BodyPart.Nuts)
                return XpPerCriticalKill;
            else
                return XpPerKill;
        }
    }
}
