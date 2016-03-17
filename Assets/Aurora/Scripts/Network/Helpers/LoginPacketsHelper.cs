// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using System.Collections.Generic;

public static class LoginPacketsHelper
{
#pragma warning disable 0168
	public static List<ServerInfo> GetServerList(this Packet packet)
	{
		var servers = new List<ServerInfo>();

		var serverCount = packet.GetByte();
		for (int i = 0; i < serverCount; ++i)
		{
			var serverName = packet.GetString();
			var unkShort1 = packet.GetShort();
			var unkShort2 = packet.GetShort();
			var unkByte2 = packet.GetByte();

			var channels = new List<ChannelInfo>();
			var channelCount = packet.GetInt();
			for (int j = 0; j < channelCount; ++j)
			{
				var channelName = packet.GetString();
				var channelState = (ChannelState)packet.GetInt();
				var channelEvent = (ChannelEvent)packet.GetInt();
				var unkInt2 = packet.GetInt();
				var stress = packet.GetShort();

				var channel = new ChannelInfo();
				channel.Name = channelName;
				channel.State = channelState;
				channel.Events = channelEvent;
				channel.Stress = stress;

				channels.Add(channel);
			}

			var server = new ServerInfo();
			server.Name = serverName;
			server.Channels.AddRange(channels);

			servers.Add(server);
		}

		return servers;
	}
#pragma warning restore 0168
}
