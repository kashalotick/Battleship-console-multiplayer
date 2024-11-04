# Battleship

My First attempt to make client-server application on C#.
Has local network multiplayer based on TCP Client and TCP Listener.
Right to make the first move is set by `_turn = new Random().Next(2);`.

### Modes
1. Client - allows to join existing server.
2. Server + Client - create server and join.
  <br>_Note: For Windows default port is 5000, for Unix (MacOS) is 5001_
3. Server only - create server.

### Controls
- WASD / ↑←↓→ - Move ship/cursor
- 1,2,3,4 - Choose ship
- R - Rotate ship
- Enter - Place ship / Shoot / Confirm
- Backspace - Delete ship
- C - Confirm position of ships

### Coming soon...
- Session save in case someone disconnects
- Language settings
- Chat

## Images
![img1.png](SeaBattle%2FResources%2Fimg1.png)![img2.png](SeaBattle%2FResources%2Fimg2.png)![img3.png](SeaBattle%2FResources%2Fimg3.png)
