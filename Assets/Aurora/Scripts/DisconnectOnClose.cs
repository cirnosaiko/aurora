// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;

public class DisconnectOnClose : MonoBehaviour
{
	void OnApplicationQuit()
	{
		Connection.Client.Disconnect();
	}
}
