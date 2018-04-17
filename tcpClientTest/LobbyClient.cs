using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace tcpClientTest {
  public class LobbyClient {
    public Socket socket;
    public Thread stream;
    public bool pinging;

    public LobbyClient (Socket s) {
      socket = s;
      pinging = false;
    }
  }
}
