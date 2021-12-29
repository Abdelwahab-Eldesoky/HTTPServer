using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            //TODO: initialize this.serverSocket
            this.LoadRedirectionRules(redirectionMatrixPath);
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(100);

            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket clientSocket = serverSocket.Accept();
                Thread newThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newThread.Start(clientSocket);
            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            Socket clientSocket = (Socket)obj;
            clientSocket.ReceiveTimeout = 0;

            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] data = new byte[1024];
                    int receivedDataLength = clientSocket.Receive(data);
                    // TODO: break the while loop if receivedLen==0
                    if (receivedDataLength == 0)
                    {
                        Console.WriteLine("Client didn't send anything");
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    Request clientRequest = new Request(Encoding.ASCII.GetString(data, 0, receivedDataLength));

                    // TODO: Call HandleRequest Method that returns the response
                    Response serverResponse = HandleRequest(clientRequest);
                    string responseStr = serverResponse.ToString();
                    byte[] responseArr = Encoding.ASCII.GetBytes(responseStr);
                    // TODO: Send Response back to client
                    clientSocket.Send(responseArr);

                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                    Logger.LogException(ex);
                }
            }

            // TODO: close client socket
            clientSocket.Close();
        }

        Response HandleRequest(Request request)
        {
            string content;
            Response response;
            try
            {
                //TODO: check for bad request 
                //TODO: map the relativeURI in request to get the physical path of the resource.
                //TODO: check for redirect
                //TODO: check file exists
                //TODO: read the physical file
                // Create OK response
                if (request.ParseRequest())
                {
                    Uri uri = new Uri(request.relativeURI);
                    string filePath = uri.LocalPath;
                    string redirectionPath = GetRedirectionPagePathIFExist(filePath);
                    content = LoadDefaultPage(filePath);
                    response = new Response(StatusCode.OK, "text/html", content, "");
                }
                else
                {
                    response = new Response(StatusCode.BadRequest, "text/html", LoadDefaultPage(Configuration.BadRequestDefaultPageName), "");
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                // TODO: in case of exception, return Internal Server Error.
                Logger.LogException(ex);
                response = new Response(StatusCode.InternalServerError, "text/html", LoadDefaultPage(Configuration.InternalErrorDefaultPageName), "");
            }
            return response;
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            LoadRedirectionRules(relativePath);
            foreach (KeyValuePair<string, string> dict in Configuration.RedirectionRules)
            {
                if (dict.Key == relativePath)
                {
                    return dict.Value;
                }
            }
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            if (!Directory.Exists(filePath))
            {
                Logger.LogException(new DirectoryNotFoundException());
                return string.Empty;
            }
            // else read file and return its content
            else
            {
                return File.ReadAllText(filePath);
            }
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                // TODO: using the filepath paramter read the redirection rules from file 
                // then fill Configuration.RedirectionRules dictionary 
                Configuration.RedirectionRules = new Dictionary<string, string>();
                StreamReader reader = new StreamReader(filePath);
                string content = reader.ReadLine();
                string[] red = content.Split(',');

                Configuration.RedirectionRules.Add(red[0], red[1]);
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}