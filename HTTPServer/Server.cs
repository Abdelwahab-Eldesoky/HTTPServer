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
            this.LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portNumber);

            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
        }

        public void StartServer()
        {
            Console.WriteLine("Start Listening...");
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
            Console.WriteLine("Connection Accepted : {0}", clientSocket.RemoteEndPoint);

            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] receivedData = new byte[65536];
                    int receivedDataLength = clientSocket.Receive(receivedData);
                    //Console.WriteLine("Client Message Received : " + clientSocket.RemoteEndPoint);

                    string requestString = Encoding.ASCII.GetString(receivedData, 0, receivedDataLength);
                    //Console.WriteLine(requestString);

                    // TODO: break the while loop if receivedLen==0
                    if (receivedDataLength == 0)
                    {
                        Console.WriteLine("Client didn't send anything");
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    Console.WriteLine(requestString);
                    Request clientRequest = new Request(requestString);

                    // TODO: Call HandleRequest Method that returns the response
                    Response serverResponse = HandleRequest(clientRequest);
                    string res = serverResponse.ResponseString;
                    Console.WriteLine(res);

                    byte[] responseArr = Encoding.ASCII.GetBytes(res);

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
            StatusCode code;
            try
            {
                //throw new Exception();
                //TODO: check for bad request 
                if (!request.ParseRequest())
                {
                    code = StatusCode.BadRequest;
                    content = "< !DOCTYPE html >< html >< body >< h1 > 400 Bad Request</ h1 >< p > 400 Bad Request</ p ></ body ></ html >";
                }
                //TODO: map the relativeURI in request to get the physical path of the resource.
                string[] name = request.relativeURI.Split('/');
                string physicalPath = Configuration.RootPath + '\\' + name[1];

                //TODO: check for redirect
                for (int i = 0; i < Configuration.RedirectionRules.Count; i++)
                {
                    if ('/' + Configuration.RedirectionRules.Keys.ElementAt(i).ToString() == request.relativeURI)
                    {
                        code = StatusCode.Redirect;
                        request.relativeURI = '/' + Configuration.RedirectionRules.Values.ElementAt(i).ToString();
                        name[1] = Configuration.RedirectionRules.Values.ElementAt(i).ToString();

                        physicalPath = Configuration.RootPath + '\\' + name[1];
                        content = File.ReadAllText(physicalPath);
                        string location = "http://localhost:1000/" + name[1];
                        response = new Response(code, "text/html", content, location);
                        return response;
                    }
                }

                //TODO: check file exists
                if (!File.Exists(physicalPath))
                {
                    physicalPath = Configuration.RootPath + '\\' + "NotFound.html";
                    code = StatusCode.NotFound;
                    content = File.ReadAllText(physicalPath);
                }

                //TODO: read the physical file
                else
                {
                    content = File.ReadAllText(physicalPath);
                    code = StatusCode.OK;
                }
                // Create OK response
                response = new Response(code, "text/html", content, physicalPath);
                return response;
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                // TODO: in case of exception, return Internal Server Error.
                Logger.LogException(ex);
                string physicalPath = Configuration.RootPath + '\\' + "InternalError.html";
                code = StatusCode.InternalServerError;
                content = File.ReadAllText(physicalPath);
                response = new Response(code, "text/html", content, physicalPath);

                return response;
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
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
