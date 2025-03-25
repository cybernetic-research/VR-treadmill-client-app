using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class tcpipserver50505 : MonoBehaviour
{
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;
    const string IpAddr = "127.0.0.1";
    [SerializeField]
    private int serverPort = 50505; // Default port, changeable via the Inspector or at runtime
    [SerializeField]
    private string serverIPAddress = IpAddr; // IP Address exposed to Unity

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serverIPAddress = GetLocalIPAddress(); // Dynamically retrieve the local IP address
        Debug.Log($"Local IP Address: {serverIPAddress}");
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToClient("Hello");
        }
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(IpAddr); // Assuming localhost for server
            server = new TcpListener(localAddr, serverPort);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);

                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(response);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server?.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        server?.Stop();
        thread?.Abort();
    }

    public void SendMessageToClient(string message)
    {
        if (stream == null)
        {
            Debug.LogWarning("Stream is null. Message cannot be sent.");
            return;
        }

        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }

    // Optionally, add a method to set the port programmatically
    public void SetPort(int port)
    {
        if (server == null) // Ensure the server isn't already running
        {
            serverPort = port;
            Debug.Log($"Server port set to {serverPort}");
        }
        else
        {
            Debug.LogWarning("Cannot change the port while the server is running.");
        }
    }

    private string GetLocalIPAddress()
    {
        string localIP = IpAddr; // Default to localhost
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // Check for IPv4 address
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error retrieving local IP address: " + e.Message);
        }
        return localIP;
    }
}
