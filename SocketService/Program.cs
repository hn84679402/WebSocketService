using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketService
{
	class Program
	{
		private static byte[] _buffer = new byte[1024];
		private static List<Socket> _clientSockets = new List<Socket>();
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		static string Recivetext = "";
		static void Main(string[] args)
		{
			Console.Title = "Server";
			//初始化聊天伺服器
			setupServer();
			Console.ReadLine();
		}
		private static void setupServer()
		{
			Console.WriteLine("Setting up server...");
			//綁定Socket
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
			//監聽數量設定 因為是測試先使用1條
			_serverSocket.Listen(1);
			//開啟Socket接通
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

		}
		private static void AcceptCallback(IAsyncResult AR)
		{
			Socket socket = _serverSocket.EndAccept(AR);
			_clientSockets.Add(socket);
			Console.WriteLine("client connect");
			//開啟回復
			socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
		}
		private static void ReceiveCallback(IAsyncResult AR)
		{
			Socket socket = (Socket)AR.AsyncState;
			int received = socket.EndReceive(AR);
			byte[] dataBuf = new byte[received];
			Array.Copy(_buffer, dataBuf, received);

			string text = Encoding.UTF8.GetString(dataBuf);

			string response = string.Empty;
			//如果不是刷新聊天頁面的請求 就要寫入伺服器 並回傳給所有使用者
			if(text.ToLower() != "get chat")
			{
				response = Recivetext += text + "\n";
				Console.WriteLine("Text received:" + text);
			}
			//刷新伺服器請求 只回傳有更新的資訊
			else
			{
				response = Recivetext += "";
			}
			byte[] data = Encoding.UTF8.GetBytes(response);
			//回傳訊息
			socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
			socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
		}

		private static void SendCallback(IAsyncResult AR)
		{
			Socket socket = (Socket)AR.AsyncState;
			socket.EndSend(AR);
		}
	}
}
