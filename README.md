## Photon Realtime Server Startup without crashing

### You must change the config file to reflect the correct location.
- PhotonRealtimeServer/bin_Win64/PhotonServer.config
  
# Change Game Server Application Type in Config
# Type="UberStrikeClassic.Realtime.Server.Game.Server.GameServerApplication"
# Needs to be changed to
# Type="UberStrikeClassic.Realtime.Server.Game.GameApplication"

## Create two folders for the compiled game files
- CommServer/bin and GameServer/bin

# Get compiled files and place them into the bin folders.
## You may also need to change the output folder for compile 

- bin/Debug
