using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpServerTest {
  public static class Server {
    private const int port = 5000;
    private static int lobby_counter = 0;
    private static List<Client> clients = new List<Client>();
    private static List<Lobby> lobbies = new List<Lobby>();
    static Thread AC;
    static Thread PCL;

    public static void Start() {
      Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
      socket.Listen(1);

      AC = new Thread(() => AcceptClients(socket));
      AC.Start();

      PCL = new Thread(PingClientList);
      PCL.Start();

      Console.WriteLine("Server Started on port: {0}", port);
    }

    private static void AcceptClients(Socket socket) {
      while (true) {
        Socket new_socket = socket.Accept();
        Console.WriteLine("Got new client!");
        Client client = new Client(new_socket, ClientType.Default);
        clients.Add(client);
        Console.WriteLine("Client Count: {0}", clients.Count);
        client.Start();
      }
    }

    public static void RemoveClient(Client client) {
      if (client.Type == ClientType.Host) {
        lobbies.RemoveAll(match: (c) => c.GetHostIP() == client.IP);
      }
      clients.Remove(client);
    }

    private static void PingClientList() {
      while(true) {
        if (clients.Count > 0) {
          foreach(Client client in clients) {
            client.StartPinging();
          }
        }
        Thread.Sleep(60000);
      }
    }

    public static void GetLobbyList(Client client) {
      Console.WriteLine("Getting Lobby List");
      string message = "LOBBY_LIST|";
      foreach(Lobby lobby in lobbies) {
        message += lobby.id + "-" + lobby.GetClientCount() + "-" + lobby.GetHostIP() + ",";
      }
      client.SendMessage(message);
    }

    public static void JoinLobby(int id, Client client) {
      foreach(Lobby lobby in lobbies) {
        if (lobby.id == id) {
          lobby.Connect(client);
          Console.WriteLine("{0} joined the lobby with id: {1}", client.IP, lobby.id);
          return;
        }
      }
      client.SendMessage("ERROR|Lobby Doesn't Exist");
    }

    public static void LeaveLobby(Client client) {
      Console.WriteLine("{0} leaving lobby", client.IP);
      foreach(Lobby lobby in lobbies) {
        if (lobby.RemoveClient(client)) {
          return;
        }
      }
    }

    public static void KillLobby(Client client) {
      Console.WriteLine("{0} killing lobby", client.IP);
      foreach (Lobby lobby in lobbies) {
        if (lobby.GetHostIP() == client.IP) {
          lobbies.Remove(lobby);
          break;
        }
      }
    }

    public static void HostLobby(Client client) {
      Lobby new_lobby = new Lobby(client, lobby_counter++);
      lobbies.Add(new_lobby);
      Console.WriteLine("{0} is now hosting lobby with the id: {1}", client.IP, new_lobby.id);
    }
  }
}
