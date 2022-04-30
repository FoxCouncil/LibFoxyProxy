using LibFoxyProxy.Security;
using System.Net.Sockets;

namespace LibFoxyProxy;

public class ListenerSocket
{
    public Socket RawSocket { get; set; }

    public bool IsSecure { get; set; } = false;

    public SslStream SecureStream { get; set; }
   
}
