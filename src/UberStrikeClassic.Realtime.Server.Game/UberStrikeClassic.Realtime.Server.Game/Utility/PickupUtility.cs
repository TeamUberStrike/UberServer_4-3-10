using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;

namespace UberStrikeClassic.Realtime.Server.Game.Utility
{
    public class PickupUtility
    {
        public const int HealthSmall = 5;

        public const int HealthMedium = 25;

        public const int HealthBig = 50;

        public const int UberHealth = 100;

        public const int PickupSmallRespawn = 5000;

        public const int PickupMediumRespawn = 15000;

        public const int PickupBigRespawn = 30000;

        public const int UberPickupRespawn = 60000;

        public const int PickupWeaponRespawn = 15000;

        public const int PickupAmmoRespawn = 10000;

        public const int ArmorSmall = 5;

        public const int ArmorMedium = 25;

        public const int ArmorBig = 50;

        public const int UberArmor = 100;

        public static int GetRespawnTime(PickupItemType type, int value)
        {
            switch (type)
            {
                case PickupItemType.Armor:
                case PickupItemType.Health:
                    if (value == HealthSmall || value == ArmorSmall)
                        return PickupSmallRespawn;
                    else if (value == HealthMedium || value == ArmorMedium)
                        return PickupMediumRespawn;
                    else if (value == HealthBig || value == ArmorBig)
                        return PickupBigRespawn;
                    else
                        return UberPickupRespawn;
                case PickupItemType.Weapon:
                    return PickupWeaponRespawn;
                case PickupItemType.Ammo:
                    return PickupAmmoRespawn;
                default:
                    return 0;
            }
        }

        public static bool IsValueValid(PickupItemType type, int value)
        {
            if (type != PickupItemType.Armor && type != PickupItemType.Health)
                return true;

            return value == HealthSmall || value == HealthMedium || value == HealthBig || value == UberHealth || value == ArmorSmall
                || value == ArmorMedium || value == ArmorBig || value == UberArmor;
        }
    }
}
