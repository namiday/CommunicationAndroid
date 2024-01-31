using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class SimpleHttpServer : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/"); // Listen on port 8080, on all network interfaces
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

        listenerThread = new Thread(StartListener);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void StartListener()
    {
        listener.Start();
        Debug.Log("Server started.");

        while (listener.IsListening)
        {
            try
            {
                Debug.Log("Listening");
                var context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex.Message}");
            }
        }

        listener.Close();
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        string dataReceived = null;

        if (request.HttpMethod == "POST")
        {
            using (Stream body = request.InputStream)
            using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
            {
                dataReceived = reader.ReadToEnd();
            }
        }

        Debug.Log($"Received request: {request.HttpMethod}, Data: {dataReceived}");

        // Prepare the response
        string responseString = "<html><body>Hello from Unity!</body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        HttpListenerResponse response = context.Response;
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    void OnApplicationQuit()
    {
        if (listener != null)
        {
            listener.Stop();
            listener.Close();
        }

        if (listenerThread != null)
        {
            listenerThread.Abort();
        }
    }
}