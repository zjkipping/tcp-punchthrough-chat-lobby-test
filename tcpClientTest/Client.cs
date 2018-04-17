using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpClientTest {
  public static class Client {
    private static Socket introducer;
    private static Lobby lobby = null;
    private static bool running = true;
    private const int HOSTPORT = 4321;
    private static IPAddress IntroIP = IPAddress.Parse("54.197.196.186");
    private const int IntroPort = 5000;

    public static void Run () {
      try {
        introducer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        introducer.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        introducer.Bind(new IPEndPoint(IPAddress.Any, HOSTPORT));
        introducer.Connect(new IPEndPoint(IntroIP, IntroPort));
      } catch (Exception e) {
        Console.WriteLine("Failed to connect... : {0}" + e);
        Console.ReadKey();
      }

      Console.WriteLine("Connected to Server!");

      Thread Responses = new Thread(IntroducerResponseStream);
      Responses.Start();

      while (running == true) {
        string input = Console.ReadLine();
        if (lobby == null) {
          if (input == "END") {
            introducer.Close();
            break;
          } else {
            if (input != "") {
              SendIntroducerMessage(input);
            }
          }
        } else {
          if (input != "") {
            lobby.SendInput(input);
          }
        }
      }
      Console.ReadKey();
    }

    private static void IntroducerResponseStream () {
      Console.WriteLine("Started Response Stream");
      while (true) {
        string response = GetResponse();
        if (response != "") {
          string[] sections = response.Split('|');
          string responseType = sections[0];
          sections[1] = sections[1].Replace("\r", "").Replace("\n", "");
          if (responseType == "PING") {
            SendIntroducerPong();
          } else if (responseType == "PUNCH-CLIENT") {
            string[] ipParts = sections[1].Split(':');
            Socket connection = PerformPunchThrough(new IPEndPoint(IPAddress.Parse(ipParts[0]), int.Parse(ipParts[1])));
            if (connection != null) {
              if (lobby != null) {
                lobby.Disconnect();
              }
              lobby = new ClientLobby(connection);
              Console.WriteLine("Connected To Lobby!");
            } else {
              lobby = null;
              Console.WriteLine("Failed to connect to Lobby...");
            }
          } else if (responseType == "PUNCH-HOST") {
            string[] ipParts = sections[1].Split(':');
            Socket connection = PerformPunchThrough(new IPEndPoint(IPAddress.Parse(ipParts[0]), int.Parse(ipParts[1])));
            if (connection != null) {
              if (lobby == null) {
                lobby = new HostLobby(connection);
              } else {
                lobby.AddClient(connection);
              }
              Console.WriteLine("Peer Connected To Lobby!");
            } else {
              Console.WriteLine("Peer Failed to connect To Lobby...");
            }
          } else if (responseType == "LOBBY_LIST") {
            Console.WriteLine("");
            Console.WriteLine("Lobby List:");
            if (sections[1] != "") {
              string[] lobbies = sections[1].Split(',');
              foreach (string lobby in lobbies) {
                if (lobby != "") {
                  string[] lobbyInfo = lobby.Split('-');
                  Console.WriteLine("id: {0} - client_count: {1} - host: {2}", lobbyInfo[0], lobbyInfo[1], lobbyInfo[2]);
                }
              }
            } else {
              Console.WriteLine("No active lobbies...");
            }
            Console.WriteLine("");
          } else {
            Console.WriteLine("{0}", response);
          }
        } else {
          introducer.Close();
          running = false;
          Console.WriteLine("Server Connection Failed...");
          break;
        }
      }
    }

    private static void SendIntroducerPong () {
      SendIntroducerMessage("PONG|");
    }

    private static string GetResponse () {
      try {
        byte[] buffer = new byte[1024];
        return Encoding.ASCII.GetString(buffer, 0, introducer.Receive(buffer));
      } catch {
        return "";
      }
    }

    private static void SendIntroducerMessage (string message) {
      try {
        introducer.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        introducer.Close();
      }
    }

    public static void LeaveLobby (bool forced) {
      if (forced) {
        Console.WriteLine("Lost Connection To Lobby...");
      }
      lobby = null;
    }

    public static void LobbyDisconnect () {
      SendIntroducerMessage("DISCONNECT");
    }

    public static void KillLobby () {
      SendIntroducerMessage("KILL_LOBBY");
    }

    public static Socket PerformPunchThrough (IPEndPoint peer) {
      Socket client = null;
      bool connected = false;
      while (!connected) {
        try {
          if (client != null) {
            client.Close();
          }
          client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
          client.Bind(new IPEndPoint(IPAddress.Any, HOSTPORT));
          client.Connect(peer);
          connected = true;
        } catch (Exception e) {
          Console.WriteLine(e);
        }
      }

      if (connected) {
        return client;
      } else {
        return null;
      }
    }
  }
}
