// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PacketHandler : MonoBehaviour
{
	public GameObject DummyCreature;

	private delegate void PacketHandlerFunc(Packet packet);

	private class PacketHandlerAttribute : Attribute
	{
		public int[] Ops { get; protected set; }

		public PacketHandlerAttribute(params int[] ops)
		{
			this.Ops = ops;
		}
	}

	private Dictionary<int, PacketHandlerFunc> _handlers = new Dictionary<int, PacketHandlerFunc>();

	void Start()
	{
		foreach (var method in this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			foreach (PacketHandlerAttribute attr in method.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
			{
				var del = (PacketHandlerFunc)Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method);
				foreach (var op in attr.Ops)
					_handlers[op] = del;
			}
		}
	}

	void Update()
	{
		HandlePackets();
	}

	private void HandlePackets()
	{
		if (Connection.Client.State != ConnectionState.Connected)
			return;

		var packets = Connection.Client.GetPacketsFromQueue();
		foreach (var packet in packets)
		{
			//Debug.Log(packet);

			PacketHandlerFunc handler;
			if (!_handlers.TryGetValue(packet.Op, out handler))
			{
				Debug.LogFormat("Unhandled packet: {0:X4} ({1})", packet.Op, Op.GetName(packet.Op));
				continue;
			}

			try
			{
				handler(packet);
			}
			catch (PacketElementTypeException ex)
			{
				Debug.LogException(ex);
				Debug.Log("Packet: " + packet);
			}
		}
	}

	private T GetComponentIn<T>(string gameObjectName) where T : MonoBehaviour
	{
		var gameObj = GameObject.Find(gameObjectName);
		if (gameObj == null)
		{
			Debug.LogError("GetComponentIn: " + gameObjectName + " not found.");
			return null;
		}

		var component = gameObj.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError("GetComponentIn: " + typeof(T).Name + " component not found.");
			return null;
		}

		return component;
	}

#pragma warning disable 0168

	// Ident -> Login
	[PacketHandler(Op.ClientIdentR)]
	private void ClientIdentR(Packet packet)
	{
		var form = GetComponentIn<LoginForm>("FrmLogin");
		if (form == null || form.State != LoginState.Ident)
		{
			Debug.Log("Received ClientIdentR outside of login or in incorrect state.");
			return;
		}

		var username = form.TxtUsername.text;
		var password = form.TxtPassword.text;

		var md5 = MD5.Create();
		var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
		var sHash = BitConverter.ToString(hash).Replace("-", "");
		var sbHash = Encoding.UTF8.GetBytes(sHash);

		packet = new Packet(Op.Login, 0);
		packet.PutByte(12); // Normal login type
		packet.PutString(username);
		packet.PutBin(sbHash);
		packet.PutBin();
		packet.PutInt(0);
		packet.PutInt(0);
		packet.PutString(Connection.Client.GetLocalIp());
		Connection.Client.Send(packet);

		form.State = LoginState.Login;
	}

	// Login -> LoggedIn | Ready
	[PacketHandler(Op.LoginR)]
	private void LoginR(Packet packet)
	{
		var form = GetComponentIn<LoginForm>("FrmLogin");
		if (form == null || form.State != LoginState.Login)
		{
			Debug.Log("Received LoginR outside of login or in incorrect state.");
			return;
		}

		var result = (LoginResult)packet.GetByte();
		if (result != LoginResult.Success)
		{
			switch (result)
			{
				case LoginResult.Message:
					var unkInt1 = packet.GetInt();
					var unkInt2 = packet.GetInt();
					var message = packet.GetString();
					form.ResetForm(message);
					break;

				case LoginResult.IdOrPassIncorrect: form.ResetForm("The username or password is incorrect."); break;
				case LoginResult.SecondaryFail: form.ResetForm("The secondary password is incorrect."); break;
				case LoginResult.AlreadyLoggedIn: form.ResetForm("This account is already logged in."); break;

				default: form.ResetForm("Login failed."); break;
			}

			return;
		}

		// Parse
		var accountName = packet.GetString();
		var accountName2 = packet.GetString();
		var sessionKey = packet.GetLong();
		var unkByte1 = packet.GetByte();
		var servers = packet.GetServerList();
		var lastLogin = packet.GetDateTime();
		var lastLogout = packet.GetDateTime();
		var unkInt3 = packet.GetInt();
		var unkByte3 = packet.GetByte();
		var unkByte4 = packet.GetByte();
		var unkInt4 = packet.GetInt();
		var unkByte5 = packet.GetByte();

		var naosSupport = packet.GetBool();
		var naosSupportExpiration = packet.GetDateTime();
		var extraStorage = packet.GetBool();
		var extraStorageExpiration = packet.GetDateTime();
		var advancedPlay = packet.GetBool();
		var advancedPlayExpiration = packet.GetDateTime();

		var unkByte6 = packet.GetByte();
		var unkByte7 = packet.GetByte();

		var inventoryPlus = packet.GetBool();
		var inventoryPlusExpiration = packet.GetDateTime();
		var premiumService = packet.GetBool();
		var premiumServiceExpiration = packet.GetDateTime();
		var vipService = packet.GetBool();
		var vipServiceExpiration = packet.GetDateTime();
		var unkPremium1 = packet.GetBool();
		var unkPremium1Expiration = packet.GetDateTime();
		var unkPremium2 = packet.GetBool();
		var unkPremium2Expiration = packet.GetDateTime();

		var unkByte8 = packet.GetByte();
		var pcCafe = packet.GetByte();
		var freeBeginnerService = packet.GetByte();

		var characters = new List<CharacterInfo>();
		var characterCount = packet.GetShort();
		for (int i = 0; i < characterCount; ++i)
		{
			var serverName = packet.GetString();
			var entityId = packet.GetLong();
			var characterName = packet.GetString();
			var deletionFlag = (DeletionFlag)packet.GetByte();
			var unkLong1 = packet.GetLong();
			var unkInt5 = packet.GetInt();
			var unkByte9 = packet.GetByte();
			var unkByte10 = packet.GetByte();
			var unkByte11 = packet.GetByte();

			var character = new CharacterInfo();
			character.Server = serverName;
			character.EntityId = entityId;
			character.Name = characterName;
			character.DeletionFlag = deletionFlag;

			characters.Add(character);
		}

		var pets = new List<CharacterInfo>();
		var petCount = packet.GetShort();
		for (int i = 0; i < petCount; ++i)
		{
			var serverName = packet.GetString();
			var entityId = packet.GetLong();
			var characterName = packet.GetString();
			var deletionFlag = (DeletionFlag)packet.GetByte();
			var unkLong2 = packet.GetLong();
			var race = packet.GetInt();
			var unkLong3 = packet.GetLong();
			var unkLong4 = packet.GetLong();
			var unkInt6 = packet.GetInt();
			var unkByte12 = packet.GetByte();

			var character = new CharacterInfo();
			character.Server = serverName;
			character.EntityId = entityId;
			character.Name = characterName;
			character.DeletionFlag = deletionFlag;

			characters.Add(character);
		}

		// Set
		Connection.AccountName = accountName;
		Connection.SessionKey = sessionKey;
		Connection.Servers.Clear();
		Connection.Servers.AddRange(servers);
		Connection.Characters.Clear();
		Connection.Characters.AddRange(characters);
		Connection.Characters.AddRange(pets);

		// Transition
		SceneManager.LoadScene("CharacterSelect");
		form.State = LoginState.LoggedIn;
	}

	[PacketHandler(Op.ChannelStatus)]
	private void ChannelStatus(Packet packet)
	{
		var list = GetComponentIn<CharacterSelectList>("LstCharacters");
		if (list == null)
		{
			Debug.Log("Received ChannelStatus outside of character selection or in incorrect state.");
			return;
		}

		var servers = packet.GetServerList();

		Connection.Servers.Clear();
		Connection.Servers.AddRange(servers);

		list.UpdateChannels();
	}

	[PacketHandler(Op.ChannelInfoRequestR)]
	private void ChannelInfoRequestR(Packet packet)
	{
		var list = GetComponentIn<CharacterSelectList>("LstCharacters");
		if (list == null || list.State != CharacterSelectState.Connecting)
		{
			Debug.Log("Received ChannelInfoRequestR outside of character selection or in incorrect state.");
			return;
		}

		var success = packet.GetBool();
		if (!success)
		{
			MsgBox.Show("No channel information received.");
			return;
		}

		var serverName = packet.GetString();
		var channelName = packet.GetString();
		var channelNumber = packet.GetShort();
		var ip1 = packet.GetString();
		var ip2 = packet.GetString();
		var port1 = packet.GetShort();
		var port2 = packet.GetShort();
		var unkInt1 = packet.GetInt();
		var creatureEntityId = packet.GetLong();

		Debug.Log("Connecting to channel at " + ip1 + ":" + port1 + "...");
		Connection.Client.ConnectAsync(ip1, port1);
	}

	[PacketHandler(Op.ChannelLoginR)]
	private void ChannelLoginR(Packet packet)
	{
		var list = GetComponentIn<CharacterSelectList>("LstCharacters");
		if (list == null || list.State != CharacterSelectState.Login)
		{
			Debug.Log("Received ChannelLoginR outside of character selection or in incorrect state.");
			return;
		}

		var success = packet.GetBool();
		if (!success)
		{
			MsgBox.Show("No channel information received.");
			return;
		}

		var creatureEntityId = packet.GetLong();
		var now = packet.GetDateTime();
		var unkInt1 = packet.GetInt();
		var unkUrl = packet.GetString();

		list.State = CharacterSelectState.LoggedIn;
	}

	[PacketHandler(Op.EnterRegion)]
	private void EnterRegion(Packet packet)
	{
		var creatureEntityId = packet.GetLong();
		var unkByte1 = packet.GetByte();
		var regionId = packet.GetInt();
		var x = packet.GetInt();
		var y = packet.GetInt();

		var list = GetComponentIn<CharacterSelectList>("LstCharacters");
		if (list != null)
			list.State = CharacterSelectState.LoggedIn;

		if (!RegionManager.Load(regionId))
			MsgBox.Show("Failed to load region, it might not exist in Aurora yet.");
	}

	[PacketHandler(Op.EnterRegionRequestR)]
	private void EnterRegionRequestR(Packet packet)
	{
		var success = packet.GetBool();
		if (!success)
		{
			MsgBox.Show("Enter region request failed.");
			return;
		}

		var creatureEntityId = packet.GetLong();
		var now = packet.GetDateTime();

		packet = new Packet(Op.ChannelCharacterInfoRequest, creatureEntityId);
		Connection.Client.Send(packet);
	}

	[PacketHandler(Op.ChannelCharacterInfoRequestR)]
	private void ChannelCharacterInfoRequestR(Packet packet)
	{
		var success = packet.GetBool();
		if (!success)
		{
			MsgBox.Show("Character info request.");
			return;
		}

		var creature = ParseCreature(packet);

		Connection.ControllingEntityId = creature.EntityId;
	}

	[PacketHandler(Op.EntitiesAppear)]
	private void EntitiesAppear(Packet packet)
	{
		var entityList = GameObject.Find("Entities").transform;
		Debug.Log("Controlling: " + Connection.ControllingEntityId.ToString("X16"));

		var entityCount = packet.GetShort();
		for (int i = 0; i < entityCount; ++i)
		{
			var type = (DataType)packet.GetShort();
			var length = packet.GetInt();
			var bin = packet.GetBin();

			var entityPacket = new Packet(bin, 0);

			if (type == DataType.Creature)
			{
				var creature = ParseCreature(entityPacket);
				var rotation = -(1f / 255 * creature.Direction * 360) + 90;

				var creatureObj = GameObject.Instantiate(DummyCreature);
				creatureObj.name = string.Format("{0} ({1:X16})", creature.Name, creature.EntityId);

				creature.Transform = creatureObj.transform;
				creature.Transform.position = new Vector3(creature.X / 100f, 0, creature.Z / 100f);
				creature.Transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
				creature.Transform.SetParent(entityList);
				Connection.Entities.Add(creature.EntityId, creature);

				var entityInfo = creatureObj.GetComponent<EntityInfo>();
				entityInfo.Id = creature.EntityId;
				entityInfo.Name = creature.Name;
				entityInfo.IsConversationNpc = creature.IsConversationNpc;

				if (creature.EntityId == Connection.ControllingEntityId)
				{
					GetComponentInChildren<InputMovement>().SetTarget(creatureObj);
					GetComponentInChildren<CameraController>().SetTarget(creatureObj);
					Debug.Log("Target: " + creatureObj.name);
				}
			}
			// items...
			// props...
		}
	}

	[PacketHandler(Op.EntitiesDisappear)]
	private void EntitiesDisappear(Packet packet)
	{
		var entityList = GameObject.Find("Entities").transform;
		var toRemove = new HashSet<long>();

		var entityCount = packet.GetShort();
		for (int i = 0; i < entityCount; ++i)
		{
			var type = (DataType)packet.GetShort();
			var entityId = packet.GetLong();

			Entity entity;
			if (Connection.Entities.TryGetValue(entityId, out entity))
			{
				GameObject.Destroy(entity.Transform.gameObject);
				Connection.Entities.Remove(entityId);
			}
		}
	}

	private Creature ParseCreature(Packet packet)
	{
		var stateEx = (CreatureStatesEx)0;

		var entityId = packet.GetLong();
		var type = (CreaturePacketType)packet.GetByte();
		var name = packet.GetString();
		var unkString1 = packet.GetString();
		var unkString2 = packet.GetString();
		var race = packet.GetInt();
		var skinColor = packet.GetByte();
		var eyeType = packet.GetShort();
		var eyeColor = packet.GetByte();
		var mouthType = packet.GetByte();
		var state = (CreatureStates)packet.GetInt();
		if (type == CreaturePacketType.Public)
		{
			stateEx = (CreatureStatesEx)packet.GetInt();
			var unkInt1 = packet.GetInt();
		}
		var height = packet.GetFloat();
		var weight = packet.GetFloat();
		var upper = packet.GetFloat();
		var lower = packet.GetFloat();
		var regionId = packet.GetInt();
		var x = packet.GetInt();
		var y = packet.GetInt();
		var direction = packet.GetByte();

		// ...

		var creature = new Creature();
		creature.EntityId = entityId;
		creature.Name = name;
		creature.RegionId = regionId;
		creature.X = x;
		creature.Z = y;
		creature.Direction = direction;
		creature.State = state;

		return creature;
	}

	[PacketHandler(Op.Walking, Op.Running)]
	private void Moving(Packet packet)
	{
		var fromX = packet.GetInt();
		var fromY = packet.GetInt();
		var toX = packet.GetInt();
		var toY = packet.GetInt();

		var walking = (packet.Op == Op.Walking);
		var x = toX / 100f;
		var y = toY / 100f;

		Entity entity;
		if (Connection.Entities.TryGetValue(packet.Id, out entity))
		{
			var controller = entity.Transform.GetComponent<CreatureController>();
			if (controller != null)
				controller.Move(new Vector3(x, 0, y), walking);
		}
	}

	[PacketHandler(Op.Chat)]
	private void Chat(Packet packet)
	{
		var chat = GetComponentIn<Chat>("Chat");
		if (chat == null)
			return;

		Entity entity;
		if (!Connection.Entities.TryGetValue(packet.Id, out entity))
			return;

		var unkByte1 = packet.GetByte();
		var name = packet.GetString();
		var message = packet.GetString();
		var hasColor = packet.GetBool();
		var color = packet.GetInt();
		var unkInt1 = packet.GetInt();
		var unkByte2 = packet.GetByte();

		if (!hasColor)
			chat.AddMessage(name, message);
		else
			chat.AddMessage(color, name, message);
	}

	[PacketHandler(Op.WhisperChat)]
	private void WhisperChat(Packet packet)
	{
		var chat = GetComponentIn<Chat>("Chat");
		if (chat == null)
			return;

		var sender = packet.GetString();
		var message = packet.GetString();

		chat.AddMessage(sender + " > " + Connection.SelectedCharacter.Name, message);
	}
#pragma warning restore 0168
}
