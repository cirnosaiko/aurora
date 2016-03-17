// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Connection
{
	public static Client Client = new Client();

	public static string AccountName;
	public static long SessionKey;
	public static List<ServerInfo> Servers = new List<ServerInfo>();
	public static List<CharacterInfo> Characters = new List<CharacterInfo>();

	public static void Reset()
	{
		Client.Disconnect();

		AccountName = null;
		SessionKey = 0;
		Servers.Clear();
		Characters.Clear();
	}
}
