using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrikeClassic.Realtime.Server.Game.Common;
using UberStrikeClassic.Realtime.Server.Game.Rooms;

namespace UberStrikeClassic.Realtime.Server.Game.GameModeStates.Infected
{
    public class GameModeStateRunning : IModeState
    {
        private GameRoom Room;

        public GameModeStateRunning(GameRoom room) 
        {
            Room = room;
        }

        public void OnEnter()
        {
            List<int> actorIds = new List<int>();

            foreach(var actor in Room.Actors) 
            {
                if (actor.isPlayer)
                    actorIds.Add(actor.ActorInfo.ActorId);
            }

            Random random = new Random();

            int randomActor = random.Next(actorIds.Count);

            int id = actorIds[randomActor];

            GameActor infected = Room.GetActor(id);

            if(infected != null) 
            {
                Room.OnPlayerKilled(infected, infected, 0, 0);
            }
            else { OnEnter(); }
        }

        public void OnExit()
        {
           
        }

        public void OnUpdate()
        {
           
        }
    }
}
