using System.Collections.Generic;
using System.Net;

namespace tcpServerTest {
  class Lobby {
    private List<Client> clients = new List<Client>();
    private Client host = null;
    public int id;
    
    public Lobby(Client h, int i) {
      host = h;
      id = i;
    }

    public void Connect(Client client) {
      clients.Add(client);
      client.SendMessage("PUNCH-CLIENT|" + host.IP.ToString());
      host.SendMessage("PUNCH-HOST|" + client.IP.ToString());
    }

    public EndPoint GetHostIP() {
      return host.IP;
    }

    public bool RemoveClient(Client c) {
      foreach(Client client in clients) {
        if (client == c) {
          clients.Remove(client);
          return true;
        }
      }
      return false;
    }

    public int GetClientCount() {
      return clients.Count;
    }
  }
}
