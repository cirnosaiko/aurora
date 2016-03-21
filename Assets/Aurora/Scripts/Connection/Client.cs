using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Net.Sockets;
using Aura.Mabi.Network;

public class Client
{
	private const int BufferSize = 1024 * 512;

	private Socket socket;

	private byte[] buffer, backBuffer;
	private int remaining, ptr;

	private Queue<Packet> queue;

	private Dictionary<int, Action<Client, Packet>> awaitedPackets;

	private object awaitLock = new object();

	public string LastError { get; private set; }
	public Exception LastException { get; private set; }

	public ConnectionState State { get; private set; }

	public event Action<Packet> Receive;

	public Client()
	{
		this.queue = new Queue<Packet>();

		this.buffer = new byte[BufferSize];
		this.backBuffer = new byte[BufferSize];

		this.awaitedPackets = new Dictionary<int, Action<Client, Packet>>();
	}

	public bool Connect(string host, int port)
	{
		if (this.State == ConnectionState.Connected)
			this.Disconnect();

		this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		this.LastError = "";
		this.State = ConnectionState.Connecting;

		var success = false;

		try
		{
			this.socket.Connect(host, port);
			this.HandShake();
			success = true;
		}
		catch (SocketException ex)
		{
			this.LastError = string.Format("{0}", ex.SocketErrorCode, ex.Message);
			this.LastException = ex;
			Debug.LogException(ex);
		}
		catch (Exception ex)
		{
			this.LastError = ex.Message;
			this.LastException = ex;
			Debug.LogException(ex);
		}

		if (!success)
			this.State = ConnectionState.Disconnected;
		else
			this.State = ConnectionState.Connected;

		return success;
	}

	public void ConnectAsync(string host, int port)
	{
		if (this.State == ConnectionState.Connected)
			this.Disconnect();

		this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		this.buffer = new byte[BufferSize];

		this.LastError = "";
		this.State = ConnectionState.Connecting;

		this.socket.BeginConnect(host, port, this.OnConnect, null);
	}

	private void OnConnect(IAsyncResult result)
	{
		var success = false;

		try
		{
			this.socket.EndConnect(result);
			this.HandShake();
			success = true;
		}
		catch (SocketException ex)
		{
			this.LastError = string.Format("{0}", ex.SocketErrorCode, ex.Message);
			this.LastException = ex;
			Debug.LogException(ex);
		}
		catch (Exception ex)
		{
			this.LastError = ex.Message;
			this.LastException = ex;
			Debug.LogException(ex);
		}

		if (!success)
			this.State = ConnectionState.Disconnected;
		else
			this.State = ConnectionState.Connected;
	}

	private void HandShake()
	{
		// Get seed
		var length = this.socket.Receive(this.buffer);
		if (length != 4)
			throw new Exception("Invalid seed length.");

		//var seed = BitConverter.ToInt32(this.buffer, 0);

		// Last 4 byte is the checksum
		this.socket.Send(new byte[] { 0x88, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 });

		// Get handshake response
		length = this.socket.Receive(this.buffer);
		if (length != 7 || this.buffer[0] != 0x88 || this.buffer[6] != 0x07) // 88 07 00 00 00 00 07
			throw new Exception("Invalid handshake response");

		this.BeginReceive();
	}

	public void Disconnect()
	{
		if (this.socket == null)
			return;

		try
		{
			this.socket.Disconnect(false);
		}
		catch { }

		try
		{
			this.socket.Shutdown(SocketShutdown.Both);
		}
		catch { }

		try
		{
			this.socket.Close();
		}
		catch { }

		this.State = ConnectionState.Disconnected;
	}

	private void BeginReceive()
	{
		this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, null);
	}

	private void OnReceive(IAsyncResult result)
	{
		try
		{
			var length = this.socket.EndReceive(result);

			if (length == 0)
			{
				this.State = ConnectionState.Disconnected;
				this.LastError = "Disconnected by the peer";
				return;
			}

			ParseTcp(length, ref this.remaining, ref this.ptr, ref this.buffer, ref this.backBuffer, HandleBuffer);

			this.BeginReceive();
		}
		catch (ObjectDisposedException)
		{
		}
		catch (SocketException ex)
		{
			//this.Disconnect();
			this.LastError = ex.Message;
			this.LastException = ex;
			//Debug.LogException(ex);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	private void HandleBuffer(byte[] buffer)
	{
		var packet = new Packet(buffer, 6);

		lock (this.queue)
			this.queue.Enqueue(packet);

		var ev = this.Receive;
		if (ev != null)
			ev(packet);

		Action<Client, Packet> handler;
		lock (this.awaitLock)
		{
			if (this.awaitedPackets.TryGetValue(packet.Op, out handler))
				this.awaitedPackets.Remove(packet.Op);
		}
		if (handler != null)
			handler(this, packet);
	}

	private static void ParseTcp(int length, ref int remaining, ref int ptr, ref byte[] buffer, ref byte[] backBuffer, Action<byte[]> handler)
	{
		var copyLength = 0;
		int read = 0;

		try
		{
			while (read < length)
			{
				// Copy header, or as many bytes as left over, to the back
				// buffer on a new packet.
				if (remaining == 0)
				{
					copyLength = Math.Min(5, length - read);
					Buffer.BlockCopy(buffer, read, backBuffer, ptr, copyLength);
					read += copyLength;
					ptr += copyLength;

					if (ptr < 4)
						break;
				}

				// Read new packet's length from header in back buffer
				if (remaining == 0)
					remaining = BitConverter.ToInt32(backBuffer, 1) - ptr;

				// Copy the whole packet to the back buffer,
				// or as many bytes from the packet as possible.
				copyLength = Math.Min(remaining, length - read);
				Buffer.BlockCopy(buffer, read, backBuffer, ptr, copyLength);
				read += copyLength;
				remaining -= copyLength;
				ptr += copyLength;

				if (remaining == 0)
				{
					var toHandle = new byte[ptr];
					Buffer.BlockCopy(backBuffer, 0, toHandle, 0, ptr);

					handler(toHandle);

					remaining = 0;
					ptr = 0;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Parsing error: " + ex.Message);
			Debug.LogError("Stacktrace: " + Environment.NewLine + ex.StackTrace);

			Debug.LogError("buffer: " + BitConverter.ToString(buffer, 0, length).Replace("-", " "));
			Debug.LogError("backBuffer: " + BitConverter.ToString(backBuffer, 0, ptr).Replace("-", " "));
			Debug.LogError("copyLength: " + copyLength);
			Debug.LogError("remaining: " + remaining);
			Debug.LogError("read: " + read);
			Debug.LogError("length: " + length);

			remaining = 0;
			ptr = 0;
		}
	}

	public void Send(byte[] data)
	{
		this.socket.Send(data);
	}

	public void Send(Packet packet)
	{
		var data = new byte[1 + 4 + 1 + packet.GetSize() + 4];

		data[0] = 0x88;
		BitConverter.GetBytes(data.Length).CopyTo(data, 1);
		data[5] = 0x03;

		packet.Build(ref data, 6);

		Send(data);
	}

	public List<Packet> GetPacketsFromQueue()
	{
		var result = new List<Packet>();

		lock (this.queue)
		{
			result.AddRange(this.queue);
			this.queue.Clear();
		}

		return result;
	}

	public List<Packet> GetPacketsFromQueue(int number)
	{
		var result = new List<Packet>();

		lock (this.queue)
		{
			for (int i = 0; i < number; ++i)
				result.Add(this.queue.Dequeue());
		}

		return result;
	}

	public void HandleResponse(Packet sendPacket, int responseOp, Action<Client, Packet> callback)
	{
		lock (awaitLock)
			awaitedPackets[responseOp] = callback;

		this.Send(sendPacket);
	}

	public byte[] GetMacAddress()
	{
		foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().Where(a => a.OperationalStatus == OperationalStatus.Up))
		{
			foreach (var ip in ni.GetIPProperties().UnicastAddresses)
			{
				if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ip.Address.Equals(((IPEndPoint)socket.LocalEndPoint).Address))
					return ni.GetPhysicalAddress().GetAddressBytes();
			}
		}

		throw new Exception("Network interface not found.");
	}

	public string GetLocalIp()
	{
		if (socket == null)
			return "?";

		return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
	}
}

public enum ConnectionState
{
	Disconnected,
	Connecting,
	Connected,
}
