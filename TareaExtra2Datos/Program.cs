using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

class MQServer
{
    private static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private const int port = 5000;
    private const int timeoutMillis = 5000;

    static void Main()
    {
        // Escuchar 
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine($"Servidor escuchando en el puerto {port}...");

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Conexión recibida de " + client.Client.RemoteEndPoint.ToString());

                // Manejo de cliente en nuevo hilo
                Thread thread = new Thread(() => HandleClient(client));
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al aceptar cliente: " + ex.Message);
            }
        }
    }

    private static void HandleClient(TcpClient client)
    {
        try
        {
            client.ReceiveTimeout = timeoutMillis;
            client.SendTimeout = timeoutMillis;

            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = request.Split('|');
            string command = parts[0];
            string response = "ERROR|Comando no válido";

            switch (command)
            {
                case "PUBLISH":
                    if (parts.Length >= 3)
                    {
                        string content = parts[2];
                        messageQueue.Enqueue(content);
                        Console.WriteLine($"[PUBLICADO] {content}"); 
                        response = "OK|Mensaje recibido";
                    }
                    break;

                case "RECEIVE":
                    if (messageQueue.TryDequeue(out string msg))
                    {
                        Console.WriteLine($"[ENTREGADO] {msg}"); 
                        response = "OK|" + msg;
                    }
                    else
                    {
                        Console.WriteLine("[VACÍO] No hay mensajes disponibles"); 
                        response = "ERROR|No hay mensajes disponibles";
                    }
                    break;
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
        catch (IOException ioEx)
        {
            Console.WriteLine("Timeout o error de E/S al manejar cliente: " + ioEx.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Excepción al manejar cliente: " + ex.Message);
        }
        finally
        {
            client.Close();
        }
    }
}
