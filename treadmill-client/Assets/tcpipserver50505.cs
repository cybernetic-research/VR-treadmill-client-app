using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unity.Android.Gradle.Manifest;

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
    [SerializeField]
    private string received = "";
    [SerializeField]
    private string sent = "";
    [SerializeField]
    private string statusMsg = "";

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

    void UpdateStatus(string input)
    {
        statusMsg = input;
        Debug.Log(statusMsg);
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

                client = server.AcceptTcpClient();

                UpdateStatus("Connected!");

                data = null;
                stream = client.GetStream();

                int i;
                try
                {
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        data = Encoding.UTF8.GetString(buffer, 0, i);
                        received = data;

                        string response = "Server response: " + data.ToString();
                        SendMessageToClient(response);
                    }
                }
                catch (Exception e2)
                {
                    UpdateStatus("SocketException: " + e2.Message);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            UpdateStatus("SocketException: " + e);
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
            UpdateStatus("Stream is null. Message cannot be sent.");
            return;
        }

        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        sent = message;
    }

    // Optionally, add a method to set the port programmatically
    public void SetPort(int port)
    {
        if (server == null) // Ensure the server isn't already running
        {
            serverPort = port;
            UpdateStatus($"Server port set to {serverPort}");
        }
        else
        {
            UpdateStatus("Cannot change the port while the server is running.");
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
            UpdateStatus("Error retrieving local IP address: " + e.Message);
        }
        return localIP;
    }
}
