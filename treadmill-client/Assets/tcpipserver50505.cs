using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unity.Android.Gradle.Manifest;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using System.Threading.Tasks;
using UnityEngine.XR;

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
   
    void PerformOperation()
    {
        if (connected)
        {
      //      SendNetworkMessage("Hello: " + ct.ToString());
      //      ct += 1;
        }
        else 
        {
            UpdateStatus("not connected");
        }
    }

    private void Update()
    {
        // Consume messages from the queue on the main thread
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            received = message; // Update serialized field safely
        }
        timer += Time.deltaTime; // Increment timer by the time elapsed since the last frame
        if (timer >= interval)
        {
            PerformOperation();
            timer = 0f; // Reset the timer
        }
    }

    void UpdateStatus(string input)
    {
        try
        {
            statusMsg = input;
            Debug.Log(statusMsg);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }




    public void SendNetworkMessage(string message)
    {
        try
        {
            System.Net.Sockets.NetworkStream ns;
            lock (client.GetStream())
            {
                ns = client.GetStream();
            }
            byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message+"\r\n\r\n");
            sent = message;
            ns.Write(bytesToSend, 0, bytesToSend.Length);
            ns.Flush();
        }
        catch (Exception ex)
        {
            UpdateStatus("transmit error " + ex.Message);
        }
    }


    private async void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(IpAddr); // Assuming localhost for server
            server = new TcpListener(localAddr, serverPort);
            server.Start();

            UpdateStatus("Server started, waiting for connection...");
            while (true)
            {
                client = await server.AcceptTcpClientAsync(); // Accept clients asynchronously
                UpdateStatus("Client connected!");
                connected = true;
                stream = client.GetStream();

                // Start reading data asynchronously
                _ = Task.Run(() => ReadDataAsync(stream));
            }
        }
        catch (SocketException e)
        {
            connected = false;
            UpdateStatus("SocketException: " + e.Message);
        }
        finally
        {
            connected = false;
            server?.Stop();
        }
    }

    private async Task ReadDataAsync(NetworkStream stream)
{
    byte[] buffer = new byte[1024];
    try
    {
        while (connected)
        {
            if (stream.CanRead)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // Asynchronous reading
                if (bytesRead == 0) break; // Connection closed

                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                lock (messageQueue)
                {
                    messageQueue.Enqueue(data); // Queue message for processing in Update()
                }
            }
        }
    }
    catch (Exception e)
    {
        connected = false;
        UpdateStatus("Read error: " + e.Message);
    }
    finally
    {
        connected = false;
        client?.Close();
    }
}


    private void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        server?.Stop();
        thread?.Abort();
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
