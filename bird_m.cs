using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Player : MonoBehaviour
{
    private const string serverIp = "127.0.0.1"; 
    private const int serverPort = 65493;     
    private TcpClient tcpClient;
    private NetworkStream stream;
    private Thread clientThread;
    private volatile bool isRunning = true;

    private float initialX;
    private float initialZ;
    private float receivedY = 0.5f; // Default value for Y position

    public float velocity = 2.4f;
    private new Rigidbody2D rigidbody;

    void Start()
    {
        try
        {
            clientThread = new Thread(ReceiveData);
            clientThread.IsBackground = true;
            clientThread.Start();

            initialX = transform.position.x;
            initialZ = transform.position.z;
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    void Update()
    {
        float newY = Mathf.Lerp(-4.5f, 4.5f, receivedY); 
        transform.position = new Vector3(initialX, newY, initialZ);
    }

    void ReceiveData()
    {
        try
        {
            tcpClient = new TcpClient(serverIp, serverPort);
            stream = tcpClient.GetStream();
            Debug.Log("Connected to Python server");

            while (isRunning)
            {
                if (stream != null && stream.DataAvailable)
                {
                    byte[] data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);
                    string response = Encoding.UTF8.GetString(data, 0, bytes);

                    if (float.TryParse(response, out float parsedY))
                    {
                        receivedY = parsedY; 
                    }
                    else
                    {
                        Debug.LogWarning("Invalid data received: " + response);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in ReceiveData: " + e.Message);
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Join();
        }

        stream?.Close();
        tcpClient?.Close();
    }
}
