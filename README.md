# Photon Realtime Server Startup without crashing

### You must change the config file to reflect the correct location.
- File: PhotonRealtimeServer\bin_Win64/PhotonServer.config
  
Change Game Server Application Type in PhotonServer.config
- OriginalPath: UberStrikeClassic.Realtime.Server.Game.Server.GameServerApplication
- NewPath: UberStrikeClassic.Realtime.Server.Game.GameApplication

### Compile project 
You may also need to change the output folder for compile
- Right Click on UberStrikeClassic.Realtime.Server.Comm and select Build
- Change output path to bin\Debug\

- Right Click on UberStrikeClassic.Realtime.Server.Comm and select Build
- Change output path to bin\Debug\

### Create two folders for the compiled game files
- CommServer\bin
- GameServer\bin

Place the compiled files into their bin folders
