using System;
using System.Net;
using System.Net.Sockets;

namespace tcpClientTest {
  public enum ClientType { HOST, DEFAULT };
  public abstract class Lobby {
    public Lobby (Socket peer) { }

    abstract public void AddClient (Socket peer);
    abstract public void Disconnect ();
    abstract public void SendInput (string input);
  }
}
