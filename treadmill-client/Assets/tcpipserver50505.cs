using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unity.Android.Gradle.Manifest;
using System.Collections.Generic;

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
    bool connected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serverIPAddress = GetLocalIPAddress(); // Dynamically retrieve the local IP address
        Debug.Log($"Local IP Address: {serverIPAddress}");
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    // Update is called once per frame
    
    private Queue<string> messageQueue = new Queue<string>();
    private float timer = 0f;
    private float interval = 5f; // 5 seconds
    private int ct = 0;
    void PerformOperation()
    {
        if (connected)
        {
            SendMessageToClient("Hello: "+ct.ToString());
            ct += 1;
        }
    }

    private void Update()
    {
        // Consume messages from the queue on the main thread
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            received = message; // Update serialized field safely
            Debug.Log("Received: " + received);
            string response = "Server response: " + message;
            SendMessageToClient(message);
        }
        timer += Time.deltaTime; // Increment timer by the time elapsed since the last frame
        if (timer >= interval)
        {
            PerformOperation();
            timer = 0f; // Reset the timer
        }
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

    private void UpdateReceived(string message)
    {
        received = message;
        Debug.Log("Received: " + received);
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
                connected = true;
                data = null;
                stream = client.GetStream();

                int i;
                try
                {
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        data = Encoding.UTF8.GetString(buffer, 0, i);

                        // Add the message to the queue
                        lock (messageQueue)
                        {
                            messageQueue.Enqueue(data);
                        }


                    }
                }
                catch (Exception e2)
                {
                    UpdateStatus("SocketException: " + e2.Message);
                }
                client.Close();
                connected = false;
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
        if (connected)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            stream.Write(msg, 0, msg.Length);
            sent = message;
        }
        else 
        {
            UpdateStatus("TX error");
        }
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
