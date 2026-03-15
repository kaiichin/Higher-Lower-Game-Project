// Based on TcpEchoServerPolling handout provided by Paul Bonsma (Saxion)

using System.Net; // For IPAddress
using System.Net.Sockets; // For TcpListener, TcpClient
using System.Text; // For Encoding

class TcpServer
{
       static void Main() {
        StartServer(50001);
    }
    static void StartServer(int port) {
        // Start listening for TCP connection requests, on the given port:
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Starting Higher/Lower Game Server on port {port}");
        Console.WriteLine("Press Q to stop the server");
        // Now we handle multiple connected clients simultaneously - 
        // we keep them in a list:
        List<TcpClient> clients = new List<TcpClient>();

        // for the secret numbers for each client, we can use a dictionary that maps the TcpClient to its secret number
        Dictionary<TcpClient, int> clientSecretNumbers = new Dictionary<TcpClient, int>();

        while (true) {
            // Note: there is no error handling in this server! Is it needed? If so, where?
            AcceptNewClients(listener, clients, clientSecretNumbers);
            HandleMessages(clients, clientSecretNumbers);
            // Clean up disconnected clients. Does this actually ever happen?!
            CleanupClients(clients, clientSecretNumbers);

            if (QuitPressed()) {
                Console.WriteLine("Stopping server");
                break;
            }
            // It's good to give the CPU a break - 10ms is enough, and still gives fast response times:
            Thread.Sleep(10);
        }
        // When stopping the server, properly clean up all resources:
        foreach (TcpClient client in clients) {
            client.Close();
        }
        listener.Stop();
        Console.WriteLine("Server stopped");
    }
    static void AcceptNewClients(TcpListener listener, List<TcpClient> clients, Dictionary<TcpClient, int> clientSecretNumbers) {
        // Pending will be true if there is an incoming connection request:
        if (listener.Pending()) {
            // ..if so, accept it and store the new TcpClient:
            // (Note that the AcceptTcpClient call is not blocking now, since we know there's a pending request!)
            TcpClient newClient = listener.AcceptTcpClient();
            clients.Add(newClient);

            //game logics: generate a random secret number for the new client and store it in the dictionary
            Random rand = new Random();
            int secretNumber = rand.Next(1, 101); // generates a random number between 1 and 100
            clientSecretNumbers.Add(newClient, secretNumber);

            Console.WriteLine($"Player connected. The Secret Number: {secretNumber}");
        }
    }
    static void HandleMessages(List<TcpClient> clients, Dictionary<TcpClient, int> clientSecretNumbers) {
        foreach (TcpClient client in clients) {
            // For each of the connected clients, we check whether there's an incoming message available:
            if (client.Available > 0) {
                // ..if so, we read exactly that many bytes into an array:
                NetworkStream stream = client.GetStream();
                int packetLength = client.Available;
                byte[] data = new byte[packetLength];
                stream.Read(data, 0, packetLength);

                // Convert the incoming byte array to a string (assuming ASCII encoding):
                string msg = Encoding.ASCII.GetString(data, 0, packetLength);

                // Now we can process the message and generate a reply based on the game logic:
                string reply = "";

                if (int.TryParse(msg.Trim(), out int guess)) {
                    int secretNumber = clientSecretNumbers[client];
                    if (guess < secretNumber) {
                        reply = "Higher\n";
                    } else if (guess > secretNumber) {
                        reply = "Lower\n";
                    } else {
                        reply = "Correct! You win!\n";
                        // Optionally, we could also generate a new secret number for the client to start a new game:
                        Random rand = new Random();
                        int newSecretNumber = rand.Next(1, 101);
                        clientSecretNumbers[client] = newSecretNumber;
                        Console.WriteLine($"Player guessed correctly! New Secret Number: {newSecretNumber}");
                    }
                } else {
                    reply = "Invalid input. Please send a number between 1 and 100.";
                }

                byte[] replyData = Encoding.ASCII.GetBytes(reply);

                // For now, we don't do anything special with the incoming message - 
                // just send it straight back to the sender:
                stream.Write(replyData, 0, replyData.Length);
                Console.WriteLine($"Player Guessed: {msg.Trim()} -> Reply: {reply.Trim()}");
            }
        }
    }
    static void CleanupClients(List<TcpClient> clients, Dictionary<TcpClient, int> clientSecretNumbers)
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            // If any of our current clients are disconnected, 
            // we close the TcpClient to clean up resources, and remove it from our list:
            // (Note that this type of for loop is needed since we're modifying the collection inside the loop!)
            if (!clients[i].Connected)
            {
                // we call RemoveAt here, since we want to remove the client from the list, but also need to close it first to clean up resources.
                clientSecretNumbers.Remove(clients[i]); 

                clients[i].Close();
                clients.RemoveAt(i);
                Console.WriteLine($"Removing players. Number of connected players: {clients.Count}");
            }
        }
    }
    static bool QuitPressed()
    {
        if (Console.KeyAvailable)
        {
            char input = Console.ReadKey(true).KeyChar;
            if (input == 'q')
            {
                return true;
            }
        }
        return false;
    }
}
            