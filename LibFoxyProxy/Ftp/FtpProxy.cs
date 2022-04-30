using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LibFoxyProxy.Ftp;

public class FtpProxy : Listener
{
    const string NewLine = "\r\n";

    public FtpProxy(IPAddress listenAddress, int port) : base(listenAddress, port, SocketType.Stream, ProtocolType.Tcp) { }

    internal override async Task<byte[]> ProcessConnection(ListenerSocket connection)
    {
        return Encoding.ASCII.GetBytes("FoxyProxy FTP Proxy" + NewLine);
    }

    internal override async Task<byte[]> ProcessRequest(ListenerSocket connection, byte[] data, int read)
    {
        var requestData = Encoding.ASCII.GetString(data, 0, read);

        // System.Diagnostics.Debugger.Break();

        await connection.RawSocket.SendAsync(Encoding.ASCII.GetBytes("FolderA\\" + NewLine + "FolderB\\" + NewLine), SocketFlags.None);

        connection.RawSocket.Close();

        return null;
    }
}
