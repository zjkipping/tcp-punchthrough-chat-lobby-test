using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpServerTest {
  public enum ClientType {
    Host,
    Default
  }

  public class Client {
    private Socket Socket = null;
    private Thread PingThread = null;
    private Thread Listening = null;
    private bool Pinging = false;
    public ClientType Type { get; private set; }

    public EndPoint IP {
      get {
        return Socket.RemoteEndPoint;
      }
      private set { }
    }

   public Client(Socket s, ClientType t) {
      Socket = s;
      Type = t;
      Listening = new Thread(Listen);
    }

    public void Start() {
      if (Socket != null) {
        Listening.Start();
      }
    }

    public void StartPinging() {
      PingThread = new Thread(PingClient);
      PingThread.Start();
    }

    public void Listen() {
      while (true) {
        byte[] buffer = new byte[1024];
        try {
          string response = Encoding.ASCII.GetString(buffer, 0, Socket.Receive(buffer));
          if (response == "") {
            Server.RemoveClient(this);
            break;
          }
          string[] sections = response.Split('|');
          string responseType = sections[0];
          if (responseType == "PONG") {
            Pinging = false;
          } else if (responseType == "REQ_LOBBIES" && Type == ClientType.Default) {
            Server.GetLobbyList(this);
          } else if (responseType == "HOST" && Type == ClientType.Default) {
            Server.HostLobby(this);
            this.Type = ClientType.Host;
          } else if (responseType == "CONNECT" && Type == ClientType.Default) {
            int id = int.Parse(sections[1]);
            Server.JoinLobby(id, this);
          } else if (responseType == "DISCONNECT" && Type == ClientType.Default) {
            Server.LeaveLobby(this);
          } else if (responseType == "KILL_LOBBY" && Type == ClientType.Host) {
            Server.KillLobby(this);
            this.Type = ClientType.Default;
          }
        } catch (SocketException) {
          Server.RemoveClient(this);
          break;
        }
      }
    }

    public void PingClient() {
      Pinging = true;
      try {
        Console.WriteLine("PING CLIENT AT {0}", Socket.RemoteEndPoint);
        Socket.Send(Encoding.ASCII.GetBytes("PING|\r\n"));
        Thread.Sleep(10000);
        if (Pinging) {
          Socket.Close();
        }
      } catch (SocketException) {
        Socket.Close();
      }
    }

    public void SendMessage(string message) {
      try {
        Socket.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
      } catch (SocketException) {
        Socket.Close();
      }
    }
  }
}
