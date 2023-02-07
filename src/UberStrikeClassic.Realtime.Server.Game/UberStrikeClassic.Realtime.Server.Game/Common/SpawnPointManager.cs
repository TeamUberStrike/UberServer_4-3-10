using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Realtime.Common;

namespace UberStrikeClassic.Realtime.Server.Game.Common
{
    public class SpawnPointManager
    {
		public Dictionary<TeamID, List<int>> SpawnPoints { get; protected set; }

		private Dictionary<TeamID, List<int>> takenSpawns;

		public bool IsLoaded { get; set; }

		public SpawnPointManager() 
		{
			SpawnPoints = new Dictionary<TeamID, List<int>>();
			SpawnPoints.Add(TeamID.NONE, new List<int>());
			SpawnPoints.Add(TeamID.RED, new List<int>());
			SpawnPoints.Add(TeamID.BLUE, new List<int>());

			IsLoaded = false;

			takenSpawns = new Dictionary<TeamID, List<int>>();
			takenSpawns.Add(TeamID.NONE, new List<int>());
			takenSpawns.Add(TeamID.RED, new List<int>());
			takenSpawns.Add(TeamID.BLUE, new List<int>());
		}

        public void AddPoints(int blue, int red, int none) 
        {
			foreach (var teamSpawn in SpawnPoints.Values)
			{
				teamSpawn.Clear();
			}

			for (int i = 0; i < none; i++)
			{
				SpawnPoints[TeamID.NONE].Add(i);
			}

			for (int i = 0; i < red; i++)
			{
				SpawnPoints[TeamID.RED].Add(i);
			}

			for (int i = 0; i < blue; i++)
			{
				SpawnPoints[TeamID.BLUE].Add(i);
			}

			IsLoaded = true;
		}

		public void ResetSpawns()
		{
			if (takenSpawns == null || takenSpawns.Count <= 0)
				return;

			foreach(var team in takenSpawns.Values)
			{
				if (team == null || team.Count <= 0) continue;

				team.Clear();
			}
		}

		public int RequestSpawnPointForTeam(TeamID team) 
		{
			if (!SpawnPoints.ContainsKey(team)) return 0;

			int spawn = -1;
			if (takenSpawns[team].Count >= SpawnPoints[team].Count)
			{
				takenSpawns[team].Clear();
			}

			do
			{
				spawn++;
			} while (takenSpawns[team].Contains(spawn) && spawn < SpawnPoints[team].Count - 1);

			takenSpawns[team].Add(spawn);

			return spawn;
		}
    }
}
