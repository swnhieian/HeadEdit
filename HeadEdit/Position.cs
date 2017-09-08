using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace HeadEdit
{
    class Position
    {
        IPAddress ip;
        Socket clientSocket;
        byte[] result;
        string total;
        public Position()
        {
            ip = IPAddress.Parse("127.0.0.1");
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            result = new byte[1024];
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 27005)); //配置服务器IP与端口  
                Console.WriteLine("连接服务器成功");
            }
            catch
            {
                Console.WriteLine("连接服务器失败");
                return;
            }

        }
        ~Position()
        {
            clientSocket.Close();
        }
        public void start(MainWindow window, ThreadDelegate method)
        {
            while (true)
            {
                int receiveLength = clientSocket.Receive(result);
                //Console.WriteLine("接收服务器消息：{0}", Encoding.ASCII.GetString(result, 0, receiveLength));
                total = Encoding.ASCII.GetString(result, 0, receiveLength);
                //Console.WriteLine(total);
                string[] spt = total.Split(',');// Regex.Split(total, ",", RegexOptions.IgnoreCase);
                double[] coord = new double[2];
                int z = 0;
                foreach (string i in spt)
                {
                    coord[z] = Convert.ToDouble(i);
                    z++;
                }
                // Console.WriteLine("x={0},y={1}",coord[0], coord[1]);
                window.Dispatcher.BeginInvoke(method, new Point(coord[0], coord[1])); // direct + indirect
            }

        }
        public void start_surf(MainWindow window, ThreadDelegate method)
        {
            while (true)
            {
                int receiveLength = clientSocket.Receive(result);
                total = Encoding.ASCII.GetString(result, 0, receiveLength);
                Console.WriteLine(total);
                string[] spt = Regex.Split(total, "js", RegexOptions.IgnoreCase);
                double[] coord = new double[2];
                int z = 0;
                foreach (string i in spt)
                {
                    coord[z] = Convert.ToDouble(i);
                }

                window.Dispatcher.BeginInvoke(method, new Point(coord[0], coord[1])); // direct + indirect
            }
        }
    }
}
