using System.Net;
using System.Net.Sockets;

namespace LibFoxyProxy;

public abstract class Listener
{
    public bool IsListening { get; internal set; }

    public IPAddress Address { get; private set; }

    public int Port { get; private set; }

    public SocketType SocketType { get; private set; }

    public ProtocolType ProtocolType { get; private set; }

    public Thread ProcessThread { get; private set; }

    public Listener(IPAddress listenAddress, int port, SocketType type, ProtocolType protocol)
    {
        Address = listenAddress;
        Port = port;
        SocketType = type;
        ProtocolType = protocol;
    }

    public void Start()
    {
        ProcessThread = new Thread(new ThreadStart(Run));
        ProcessThread.Start();
    }

    private async void Run()
    {
        if (IsListening)
        {
            throw new Exception("Starting a Listener while it's already listening!");
        }

        IsListening = true;

        using var socket = new Socket(SocketType, ProtocolType);

        socket.Bind(new IPEndPoint(Address, Port));

        socket.Listen();

        Console.WriteLine("Starting server...");

        while (IsListening)
        {
            var connection = await socket.AcceptAsync();

            _ = Task.Run(async () =>
            {
                Console.WriteLine("Connection Accepted");

                var buffer = new byte[4096];

                try
                {
                    while (true)
                    {
                        if (!connection.Connected)
                        {
                            break;
                        }

                        int read = await connection.ReceiveAsync(buffer, SocketFlags.None);

                        if (read == 0)
                        {
                            break;
                        }

                        ProcessRequest(connection, buffer, read);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Connection Exception");
                }
                finally
                {
                    connection.Dispose();
                }

                Console.WriteLine("Connection Closed");
            });
        }

        Console.WriteLine("Stopping server...");

        IsListening = false;
    }

    internal virtual void ProcessRequest(Socket? connection, byte[] data, int read)
    {
    }
}
