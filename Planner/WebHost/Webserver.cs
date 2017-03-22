﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Planner;
using System.Threading;
using TaskFunctions;

namespace WebHost
{
    class WebServer
    {
        List<ClientHandler> clients = new List<ClientHandler>();
        TcpListener serverSocket;
        TaskCollection tasks;

        public WebServer(TaskCollection tasks)
        {
            this.tasks = tasks;
        }

        public void Start(int port)
        {
            serverSocket = new TcpListener(IPAddress.Any, port);
            TcpClient clientSocket;

            int clientCount = 0;

            try
            {
                serverSocket.Start();
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                ClientHandler client = new ClientHandler(clientCount);
                client.RecievedJson += new EventHandler<RecievedJsonEventArgs>(HandleJson);
                clientCount++;

                clients.Add(client);
                client.StartClient(clientSocket, tasks);
            }

            clientSocket.Close();
            serverSocket.Stop();
        }

        private void HandleJson(object sender, RecievedJsonEventArgs e)
        {
            int senderID = e.senderID;
            foreach (ClientHandler c in clients)
            {
                if (c.ID != senderID)
                {
                    c.SendData(e.obj.ToString());
                }
            }
        }
    }
}