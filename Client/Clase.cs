using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class MQClient
{
    private string ip;
    private int port;
    private Guid appId;
    private const int maxRetries = 2;
    private const int retryDelayMillis = 8000; // 8 segundos
    private const int timeoutMillis = 5000; // 5 segundos de timeout

    public MQClient(string ip, int port, Guid appId)
    {
        this.ip = ip;
        this.port = port;
        this.appId = appId;
    }

    public bool Publish(string content)
    {
        string msg = $"PUBLISH|{appId}|{content}";
        return SendMessageWithRetry(msg);
    }

    public string Receive()
    {
        string message = $"RECEIVE|{appId}";
        if (SendMessageWithRetry(message, out string response))
        {
            var parts = response.Split('|');
            if (parts[0] == "OK")
            {
                return parts[1];
            }
            throw new Exception("Respuesta de error del servidor: " + response);
        }
        throw new Exception("No se pudo enviar el mensaje tras varios intentos");
    }

    private bool SendMessageWithRetry(string message)
    {
        return SendMessageWithRetry(message, out _);
    }

    private bool SendMessageWithRetry(string message, out string response)
    {
        response = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect(ip, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeoutMillis);

                if (!success)
                {
                    throw new TimeoutException("Timeout al intentar conectar con el servidor.");
                }

                client.EndConnect(result);
                client.ReceiveTimeout = timeoutMillis;
                client.SendTimeout = timeoutMillis;

                using var stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Respuesta recibida: {response}");  // Mostrar la respuesta del servidor
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Intento {attempt} fallido: {ex.Message}");
                if (attempt < maxRetries)
                {
                    Console.WriteLine($"Reintentando en {retryDelayMillis / 1000} segundos...");
                    Thread.Sleep(retryDelayMillis);
                }
            }
        }

        return false;
    }
}

class Program
{
    static void Main(string[] args)
    {
        string serverIp = "192.168.18.4";  
        int serverPort = 5000;
        Guid appId = Guid.NewGuid(); 

        MQClient client = new MQClient(serverIp, serverPort, appId);

        while (true)
        {
            // Leer un mensaje de la consola
            Console.WriteLine("Escribe un mensaje para enviar al servidor (o 'salir' para terminar):");
            string userMessage = Console.ReadLine();

            if (userMessage.ToLower() == "salir")
            {
                break; // Salir del bucle y terminar el programa
            }

            // Publicar el mensaje
            bool publishResult = client.Publish(userMessage);
            if (publishResult)
            {
                Console.WriteLine("Mensaje publicado correctamente.");
            }

            // Intentar recibir un mensaje
            try
            {
                string receivedMessage = client.Receive();
                Console.WriteLine($"Mensaje recibido del servidor: {receivedMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recibir mensaje: {ex.Message}");
            }
        }
    }
}
