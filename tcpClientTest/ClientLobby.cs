using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpClientTest {
  public class ClientLobby : Lobby {
    private Socket connection = null;
    private Thread responseStream = null;

    public ClientLobby (Socket peer) : base(peer) {
      connection = peer;
      responseStream = new Thread(LobbyResponseStream);
      responseStream.Start();
    }

    public override void AddClient (Socket peer) { }

    override public void Disconnect () {
      if (connection != null) {
        connection.Close();
      }
    }

    override public void SendInput (string input) {
      if (input == "DISCONNECT") {
        connection.Close();
        Console.WriteLine("Left Lobby!");
        Client.LeaveLobby(false);
        Client.LobbyDisconnect();
      } else {
        SendMessage("MESSAGE|" + input);
      }
    }

    private void LobbyResponseStream () {
      while (true) {
        string response = GetResponse();
        if (response != "") {
          string[] sections = response.Split('|');
          string responseType = sections[0];
          if (responseType == "PING") {
            SendMessage("PONG|");
          } else if (responseType == "MESSAGE") {
            Console.WriteLine(sections[1]);
          }
        } else {
          connection.Close();
          Client.LeaveLobby(true);
          break;
        }
      }
    }

    private string GetResponse () {
      try {
        byte[] buffer = new byte[1024];
        return Encoding.ASCII.GetString(buffer, 0, connection.Receive(buffer));
      } catch {
        return "";
      }
    }

    private void SendMessage (string message) {
      try {
        connection.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        connection.Close();
        Client.LeaveLobby(true);
      }
    }
  }
}
