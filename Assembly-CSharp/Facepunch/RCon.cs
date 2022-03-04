using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ConVar;
using Facepunch.Rcon;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using UnityEngine;

namespace Facepunch
{
	public class RCon
	{
		public struct Command
		{
			public IPAddress Ip;

			public int ConnectionId;

			public string Name;

			public string Message;

			public int Identifier;
		}

		public enum LogType
		{
			Generic = 0,
			Error = 1,
			Warning = 2,
			Chat = 3,
			Report = 4
		}

		public struct Response
		{
			public string Message;

			public int Identifier;

			[JsonConverter(typeof(StringEnumConverter))]
			public LogType Type;

			public string Stacktrace;
		}

		internal struct BannedAddresses
		{
			public IPAddress addr;

			public float banTime;
		}

		internal class RConClient
		{
			private Socket socket;

			private bool isAuthorised;

			private string connectionName;

			private int lastMessageID = -1;

			private bool runningConsoleCommand;

			private bool utf8Mode;

			internal RConClient(Socket cl)
			{
				socket = cl;
				socket.NoDelay = true;
				connectionName = socket.RemoteEndPoint.ToString();
			}

			internal bool IsValid()
			{
				return socket != null;
			}

			internal void Update()
			{
				if (socket == null)
				{
					return;
				}
				if (!socket.Connected)
				{
					Close("Disconnected");
					return;
				}
				int available = socket.Available;
				if (available < 14)
				{
					return;
				}
				if (available > 4096)
				{
					Close("overflow");
					return;
				}
				byte[] buffer = new byte[available];
				socket.Receive(buffer);
				using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer, false), utf8Mode ? Encoding.UTF8 : Encoding.ASCII))
				{
					int num = binaryReader.ReadInt32();
					if (available < num)
					{
						Close("invalid packet");
						return;
					}
					lastMessageID = binaryReader.ReadInt32();
					int type = binaryReader.ReadInt32();
					string msg = ReadNullTerminatedString(binaryReader);
					ReadNullTerminatedString(binaryReader);
					if (!HandleMessage(type, msg))
					{
						Close("invalid packet");
					}
					else
					{
						lastMessageID = -1;
					}
				}
			}

			internal bool HandleMessage(int type, string msg)
			{
				if (!isAuthorised)
				{
					return HandleMessage_UnAuthed(type, msg);
				}
				if (type == SERVERDATA_SWITCH_UTF8)
				{
					utf8Mode = true;
					return true;
				}
				if (type == SERVERDATA_EXECCOMMAND)
				{
					Debug.Log("[RCON][" + connectionName + "] " + msg);
					runningConsoleCommand = true;
					ConsoleSystem.Run(ConsoleSystem.Option.Server, msg);
					runningConsoleCommand = false;
					Reply(-1, SERVERDATA_RESPONSE_VALUE, "");
					return true;
				}
				if (type == SERVERDATA_RESPONSE_VALUE)
				{
					Reply(lastMessageID, SERVERDATA_RESPONSE_VALUE, "");
					return true;
				}
				Debug.Log("[RCON][" + connectionName + "] Unhandled: " + lastMessageID + " -> " + type + " -> " + msg);
				return false;
			}

			internal bool HandleMessage_UnAuthed(int type, string msg)
			{
				if (type != SERVERDATA_AUTH)
				{
					BanIP((socket.RemoteEndPoint as IPEndPoint).Address, 60f);
					Close("Invalid Command - Not Authed");
					return false;
				}
				Reply(lastMessageID, SERVERDATA_RESPONSE_VALUE, "");
				isAuthorised = Password == msg;
				if (!isAuthorised)
				{
					Reply(-1, SERVERDATA_AUTH_RESPONSE, "");
					BanIP((socket.RemoteEndPoint as IPEndPoint).Address, 60f);
					Close("Invalid Password");
					return true;
				}
				Reply(lastMessageID, SERVERDATA_AUTH_RESPONSE, "");
				Debug.Log("[RCON] Auth: " + connectionName);
				Output.OnMessage += Output_OnMessage;
				return true;
			}

			private void Output_OnMessage(string message, string stacktrace, UnityEngine.LogType type)
			{
				if (isAuthorised && IsValid())
				{
					if (lastMessageID != -1 && runningConsoleCommand)
					{
						Reply(lastMessageID, SERVERDATA_RESPONSE_VALUE, message);
					}
					Reply(0, SERVERDATA_CONSOLE_LOG, message);
				}
			}

			internal void Reply(int id, int type, string msg)
			{
				MemoryStream memoryStream = new MemoryStream(1024);
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					if (utf8Mode)
					{
						byte[] bytes = Encoding.UTF8.GetBytes(msg);
						int value = 10 + bytes.Length;
						binaryWriter.Write(value);
						binaryWriter.Write(id);
						binaryWriter.Write(type);
						binaryWriter.Write(bytes);
					}
					else
					{
						int value2 = 10 + msg.Length;
						binaryWriter.Write(value2);
						binaryWriter.Write(id);
						binaryWriter.Write(type);
						foreach (char c in msg)
						{
							binaryWriter.Write((sbyte)c);
						}
					}
					binaryWriter.Write((sbyte)0);
					binaryWriter.Write((sbyte)0);
					binaryWriter.Flush();
					try
					{
						socket.Send(memoryStream.GetBuffer(), (int)memoryStream.Position, SocketFlags.None);
					}
					catch (Exception ex)
					{
						Debug.LogWarning("Error sending rcon reply: " + ex);
						Close("Exception");
					}
				}
			}

			internal void Close(string strReasn)
			{
				Output.OnMessage -= Output_OnMessage;
				if (socket != null)
				{
					Debug.Log("[RCON][" + connectionName + "] Disconnected: " + strReasn);
					socket.Close();
					socket = null;
				}
			}

			internal string ReadNullTerminatedString(BinaryReader read)
			{
				string text = "";
				do
				{
					if (read.BaseStream.Position == read.BaseStream.Length)
					{
						return "";
					}
					char c = read.ReadChar();
					if (c == '\0')
					{
						return text;
					}
					text += c;
				}
				while (text.Length <= 8192);
				return string.Empty;
			}
		}

		internal class RConListener
		{
			private TcpListener server;

			private List<RConClient> clients = new List<RConClient>();

			internal RConListener()
			{
				IPAddress address = IPAddress.Any;
				if (!IPAddress.TryParse(Ip, out address))
				{
					address = IPAddress.Any;
				}
				server = new TcpListener(address, Port);
				try
				{
					server.Start();
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Couldn't start RCON Listener: " + ex.Message);
					server = null;
				}
			}

			internal void Shutdown()
			{
				if (server != null)
				{
					server.Stop();
					server = null;
				}
			}

			internal void Cycle()
			{
				if (server != null)
				{
					ProcessConnections();
					RemoveDeadClients();
					UpdateClients();
				}
			}

			private void ProcessConnections()
			{
				if (!server.Pending())
				{
					return;
				}
				Socket socket = server.AcceptSocket();
				if (socket != null)
				{
					IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
					if (Interface.CallHook("OnRconConnection", iPEndPoint.Address) != null)
					{
						socket.Close();
					}
					else if (IsBanned(iPEndPoint.Address))
					{
						Debug.Log("[RCON] Ignoring connection - banned. " + iPEndPoint.Address.ToString());
						socket.Close();
					}
					else
					{
						clients.Add(new RConClient(socket));
					}
				}
			}

			private void UpdateClients()
			{
				foreach (RConClient client in clients)
				{
					client.Update();
				}
			}

			private void RemoveDeadClients()
			{
				clients.RemoveAll((RConClient x) => !x.IsValid());
			}
		}

		public static string Password = "";

		[ServerVar]
		public static int Port = 0;

		[ServerVar]
		public static string Ip = "";

		[ServerVar(Help = "If set to true, use websocket rcon. If set to false use legacy, source engine rcon.")]
		public static bool Web = true;

		[ServerVar(Help = "If true, rcon commands etc will be printed in the console")]
		public static bool Print = false;

		internal static RConListener listener = null;

		internal static Listener listenerNew = null;

		private static Queue<Command> Commands = new Queue<Command>();

		private static float lastRunTime = 0f;

		internal static List<BannedAddresses> bannedAddresses = new List<BannedAddresses>();

		private static int responseIdentifier;

		private static int responseConnection;

		private static bool isInput;

		internal static int SERVERDATA_AUTH = 3;

		internal static int SERVERDATA_EXECCOMMAND = 2;

		internal static int SERVERDATA_AUTH_RESPONSE = 2;

		internal static int SERVERDATA_RESPONSE_VALUE = 0;

		internal static int SERVERDATA_CONSOLE_LOG = 4;

		internal static int SERVERDATA_SWITCH_UTF8 = 5;

		public static void Initialize()
		{
			if (Interface.CallHook("IOnRconInitialize") != null)
			{
				return;
			}
			if (Port == 0)
			{
				Port = Server.port;
			}
			Password = CommandLine.GetSwitch("-rcon.password", CommandLine.GetSwitch("+rcon.password", ""));
			if (Password == "password" || Password == "")
			{
				return;
			}
			Output.OnMessage += OnMessage;
			if (Web)
			{
				listenerNew = new Listener();
				if (!string.IsNullOrEmpty(Ip))
				{
					listenerNew.Address = Ip;
				}
				listenerNew.Password = Password;
				listenerNew.Port = Port;
				listenerNew.SslCertificate = CommandLine.GetSwitch("-rcon.ssl", null);
				listenerNew.SslCertificatePassword = CommandLine.GetSwitch("-rcon.sslpwd", null);
				listenerNew.OnMessage = delegate(IPAddress ip, int id, string msg)
				{
					lock (Commands)
					{
						Command item = JsonConvert.DeserializeObject<Command>(msg);
						item.Ip = ip;
						item.ConnectionId = id;
						Commands.Enqueue(item);
					}
				};
				listenerNew.Start();
				Debug.Log("WebSocket RCon Started on " + Port);
			}
			else
			{
				listener = new RConListener();
				Debug.Log("RCon Started on " + Port);
				Debug.Log("Source style TCP Rcon is deprecated. Please switch to Websocket Rcon before it goes away.");
			}
		}

		public static void Shutdown()
		{
			if (listenerNew != null)
			{
				listenerNew.Shutdown();
				listenerNew = null;
			}
			if (listener != null)
			{
				listener.Shutdown();
				listener = null;
			}
		}

		public static void Broadcast(LogType type, object obj)
		{
			if (listenerNew != null)
			{
				Response response = default(Response);
				response.Identifier = -1;
				response.Message = JsonConvert.SerializeObject(obj, Formatting.Indented);
				response.Type = type;
				if (responseConnection < 0)
				{
					listenerNew.BroadcastMessage(JsonConvert.SerializeObject(response, Formatting.Indented));
				}
				else
				{
					listenerNew.SendMessage(responseConnection, JsonConvert.SerializeObject(response, Formatting.Indented));
				}
			}
		}

		public static void Update()
		{
			lock (Commands)
			{
				while (Commands.Count > 0)
				{
					OnCommand(Commands.Dequeue());
				}
			}
			if (listener == null || lastRunTime + 0.02f >= UnityEngine.Time.realtimeSinceStartup)
			{
				return;
			}
			lastRunTime = UnityEngine.Time.realtimeSinceStartup;
			try
			{
				bannedAddresses.RemoveAll((BannedAddresses x) => x.banTime < UnityEngine.Time.realtimeSinceStartup);
				listener.Cycle();
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Rcon Exception");
				Debug.LogException(exception);
			}
		}

		public static void BanIP(IPAddress addr, float seconds)
		{
			RCon.bannedAddresses.RemoveAll((BannedAddresses x) => x.addr == addr);
			BannedAddresses bannedAddresses = default(BannedAddresses);
			bannedAddresses.addr = addr;
			bannedAddresses.banTime = UnityEngine.Time.realtimeSinceStartup + seconds;
		}

		public static bool IsBanned(IPAddress addr)
		{
			return bannedAddresses.Count((BannedAddresses x) => x.addr == addr && x.banTime > UnityEngine.Time.realtimeSinceStartup) > 0;
		}

		private static void OnCommand(Command cmd)
		{
			try
			{
				responseIdentifier = cmd.Identifier;
				responseConnection = cmd.ConnectionId;
				isInput = true;
				if (Print)
				{
					Debug.Log(string.Concat("[rcon] ", cmd.Ip, ": ", cmd.Message));
				}
				isInput = false;
				string text = ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), cmd.Message);
				if (text != null)
				{
					OnMessage(text, string.Empty, UnityEngine.LogType.Log);
				}
			}
			finally
			{
				responseIdentifier = 0;
				responseConnection = -1;
			}
		}

		private static void OnMessage(string message, string stacktrace, UnityEngine.LogType type)
		{
			if (!isInput && listenerNew != null)
			{
				Response response = default(Response);
				response.Identifier = responseIdentifier;
				response.Message = message;
				response.Stacktrace = stacktrace;
				response.Type = LogType.Generic;
				if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
				{
					response.Type = LogType.Error;
				}
				if (type == UnityEngine.LogType.Assert || type == UnityEngine.LogType.Warning)
				{
					response.Type = LogType.Warning;
				}
				if (responseConnection < 0)
				{
					listenerNew.BroadcastMessage(JsonConvert.SerializeObject(response, Formatting.Indented));
				}
				else
				{
					listenerNew.SendMessage(responseConnection, JsonConvert.SerializeObject(response, Formatting.Indented));
				}
			}
		}
	}
}
