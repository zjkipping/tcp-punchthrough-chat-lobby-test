using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpClientTest {
  public class HostLobby : Lobby {
    private List<LobbyClient> clients = new List<LobbyClient>();

    public HostLobby (Socket peer) : base(peer) {
      AddClient(peer);
    }

    override public void AddClient (Socket peer) {
      LobbyClient new_client = new LobbyClient(peer);
      new_client.stream = new Thread(() => ClientStream(new_client));
      new_client.stream.Start();
      clients.Add(new_client);
    }

    override public void Disconnect () {
      foreach (LobbyClient client in clients) {
        if (client.socket.Connected) {
          client.socket.Close();
        }
      }
      clients.Clear();
    }

    override public void SendInput (string input) {
      if (input == "DISCONNECT") {
        Disconnect();
        Console.WriteLine("Left Lobby!");
        Client.LeaveLobby(false);
        Client.KillLobby();
      } else {
        BroadCastMessage(input, null);
      }
    }

    private void ClientStream (LobbyClient client) {
      while (true) {
        string response = GetResponse(client.socket);
        if (response != "") {
          string[] sections = response.Split('|');
          string responseType = sections[0];
          if (responseType == "PONG") {
            Console.WriteLine("Got Pong from: {0}", client.socket.RemoteEndPoint);
            client.pinging = false;
          } else if (responseType == "MESSAGE") {
            BroadCastMessage(sections[1], client.socket);
            Console.WriteLine(client.socket.RemoteEndPoint + ": " + sections[1]);
          }
        } else {
          clients.Remove(client);
          break;
        }
      }
    }

    private void PingClients () {
      while (true) {
        if (clients.Count > 0) {
          foreach (LobbyClient client in clients) {
            new Thread(() => PingClient(client)).Start();
          }
        }
        Thread.Sleep(60000);
      }
    }

    private void PingClient (LobbyClient client) {
      client.pinging = true;
      Console.WriteLine("Pinging Client: {0}", client.socket.RemoteEndPoint);
      try {
        client.socket.Send(Encoding.ASCII.GetBytes("PING|\r\n"));
        Thread.Sleep(10000);
        if (client.pinging) {
          client.socket.Close();
        }
      } catch (SocketException) {
        client.socket.Close();
      }
    }

    private string GetResponse (Socket socket) {
      try {
        byte[] buffer = new byte[1024];
        return Encoding.ASCII.GetString(buffer, 0, socket.Receive(buffer));
      } catch {
        return "";
      }
    }

    private void BroadCastMessage (string message, Socket sender) {
      string name = (sender == null) ? "HOST" : sender.RemoteEndPoint.ToString();
      foreach (LobbyClient client in clients) {
        try {
          if (sender == null || (sender != null && client.socket.RemoteEndPoint != sender.RemoteEndPoint)) {
            client.socket.Send(Encoding.ASCII.GetBytes("MESSAGE|" + name + ": " + message + "\r\n"));
          }
        } catch (SocketException) {
          client.socket.Close();
          clients.Remove(client);
        }
      }
    }
  }
}
