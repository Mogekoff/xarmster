using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    class server
    {
        static int port = 9999;
        static Thread connector = new Thread(new ThreadStart(Connector));
        static Thread checker = new Thread(new ThreadStart(Checker));
        static List<Socket> Connections;
        static Socket listenSocket;
        static IPEndPoint ipPoint;
        static string usageInfo = "Список доступных команд:\n\t\t/help\t\t-\tвыводит эту справку\n\t\t/list\t\t-\tвывод списка клиентов\n\t\t/[№] [cmd]\t-\tотправить команду по номеру клиента";
        static void Main(string[] args)
        {
            ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(ipPoint);
            listenSocket.Listen(10);
            Connections = new List<Socket>();
            
            connector.Start();
            checker.Start();
            

            WriteMessage("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                string Cmd = Console.ReadLine();
                if(string.IsNullOrEmpty(Cmd)) {
                    Console.Write('>');
                    continue;
                }
                    
                if (Cmd[0] == '/')
                {
                    Command(Cmd);
                    continue;
                }
                Console.Write('>');
                
                for (int i = 0; i < Connections.Count; i++)
                {
                    Send(i+1, Cmd);
                }

            }

        }
        static void WriteMessage(string Message) {
            Console.Write("\b["+DateTime.Now.ToLongTimeString() + "] " + Message + "\n>");
        }
        static void Connector()
        {
            while(true) {
                Socket handler = listenSocket.Accept();
                Connections.Add(handler);
                Thread receiver = new Thread(new ParameterizedThreadStart(Receiver));
                receiver.Start(handler);
                WriteMessage("Подключился новый клиент №" + Connections.Count + ": " + IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()) + " (Всего " + (Connections.Count) + ')');
            }
        }
        static void Checker()
        {
            while (true)
            {
                Thread.Sleep(5000);
                for (int i = 0; i < Connections.Count; i++)
                {
                    if (Connections[i].Poll(-1, SelectMode.SelectRead) && Connections[i].Available == 0)
                    {
                        WriteMessage("Клиент №" + (i + 1) + ' ' + IPAddress.Parse(((IPEndPoint)Connections[i].RemoteEndPoint).Address.ToString()) + " разорвал соединение. (Всего " + (Connections.Count - 1) + ')');
                        Connections[i].Shutdown(SocketShutdown.Both);
                        Connections[i].Close();
                        Connections.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        
        static void Receiver(object id) {
            Socket Client = (Socket)id;
            try
            {
                while (true)
                {
                    
                    byte[] data = new byte[8192];
                    int bytes = Client.Receive(data, data.Length, 0); 
                    WriteMessage("Вывод от клиента " + IPAddress.Parse(((IPEndPoint)Client.RemoteEndPoint).Address.ToString()) + ":\n" + Encoding.Unicode.GetString(data, 0, bytes));
                }
            }
            catch (Exception)
            {
                return;
            }
            
        }
        static void Command(string Cmd){
            Cmd = Cmd.Remove(0,1);
            switch (Cmd)
            {
                case "help":
                    WriteMessage(usageInfo);
                    break;
                case "list":
                    if (Connections.Count == 0)
                        WriteMessage("Отсутствуют текущие подключения.");
                    else {
                        string list = "Текущие подключения:\n";
                        for (int i = 0; i < Connections.Count; i++)
                            list += "\t\t№" + (i+1) + "\t\t" + IPAddress.Parse(((IPEndPoint)Connections[i].RemoteEndPoint).Address.ToString()) + "\n";
                        WriteMessage(list);
                    }
                    break;
                default:
                    if (Cmd.IndexOf(' ') != -1) {
                        Send(int.Parse(Cmd.Substring(0, Cmd.IndexOf(' ') + 1)), Cmd.Substring(Cmd.IndexOf(' ') + 1, Cmd.Length - Cmd.IndexOf(' ') - 1));
                    }
                    else {
                        WriteMessage("Нет такой команды \"" + Cmd + "\"");
                    }
                    break;
            }
        }
        static void Send(int id, string Cmd) {
            if(id>Connections.Count || id<1) {
                WriteMessage("Клиента с таким номером не существует.");
            }
            else
            {
                Connections[id-1].Send(Encoding.Unicode.GetBytes(Cmd));
            }
        }
    }
}