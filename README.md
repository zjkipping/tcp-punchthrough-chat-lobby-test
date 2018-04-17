## General
  The project was made in an attempt to learn how to do P2P through TCP punchthrough. It was a relative success on that front. This was in the pursuit of learning and I believe I learned a lot from this project.

  I feel that I should make use of the Async aspects of Sockets or at the very least learn how to implement it in case I ever want to go that route (instead of threading synchronous functionality). I also believe it would have been better to implement this application as a Windows Forms project instead of a simple console application. The project works well enough with the console, I just don't error check the inputs or sanitize the messages at all, since this was merely a test.

  ## Running
   * The only things to be changed would be the Introducer's ip/port and the port you want to connect to it from (the open tcp port on your clients).
   * I used a Windows Server EC2 instance from amazon for the introducer for free; so, I would recommend checking that out.
   * Commands are run through the console (see below) for initially the introducer and then it swaps over to the lobby once you join one (or host).
   * The server runs on it's own once you start it and outputs some general debug information

   ## Commands

   ### Introducer Interface
     Commands to the Introducer (server) are ended with a '|' character, except for general commands that are just plain text with no denoting end character
     
     - END - Used to disconnect from the introducer and stop the client program
     - REQ_LOBBIES| - Used to request the lobby list from the introducer
     - HOST| - Used to host/form a lobby
     - CONNECT|# - Used to connect to a lobby, where the '#' is replaced with the lobby ID
   ### Lobby Interface
     Commands to the lobby host server still end with the '|' character, but aren't used through the chat system
     
     - DISCONNECT - Used to disconnect from the lobby (client) or leave/kill the lobby (host)

  Other general commands are used throughout the program such as PING|, PONG|, MESSAGE|, etc...
  These are more or less in the background and are never used by the console user.

  The next step it to re-do this in a form environment and clean things up as an actual example of how to do a TCP Punch Through with an Introducer server.
