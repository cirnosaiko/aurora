// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Mabi;

public class CharacterSelectList : MonoBehaviour
{
	public Transform ButtonsParent;
	public GameObject ButtonReference;
	public Dropdown SelChannel;
	public Button BtnStart;
	public Text TxtSelectedChar;

	[HideInInspector]
	public CharacterSelectState State;

	private CharacterInfo[] characters;
	private List<ChannelInfo> channels = new List<ChannelInfo>();
	private CharacterInfo selectedCharacter;
	private ChannelInfo selectedChannel;
	private CharacterSelectState prevState;
	private MsgBox stateInfo;

	void Start()
	{
		characters = Connection.Characters.OrderBy(a => a.EntityId).ToArray();
		foreach (var character in characters)
		{
			var entityId = character.EntityId;
			var name = string.Format("{0} ({1})", character.Name, character.Server);

			var buttonObj = GameObject.Instantiate(ButtonReference);
			buttonObj.transform.SetParent(ButtonsParent);

			var text = buttonObj.GetComponentInChildren<Text>();
			text.text = name;

			var disable = buttonObj.transform.FindChild("Img" + (character.IsPet ? "Character" : "Pet"));
			disable.gameObject.SetActive(false);

			var button = buttonObj.GetComponent<Button>();
			button.onClick.AddListener(() => { OnCharacterSelected(entityId); });
		}

		BtnStart.interactable = false;
		BtnStart.onClick.AddListener(BtnStart_OnClick);

		SelChannel.interactable = false;
		SelChannel.onValueChanged.AddListener(SelChannel_OnValueChanged);
		SelChannel.ClearOptions();

		TxtSelectedChar.gameObject.SetActive(characters.Length != 0);
		if (characters.Length != 0)
			OnCharacterSelected(characters[0].EntityId);
	}

	void Update()
	{
		var connecting = false;
		if (prevState != State && State == CharacterSelectState.Connecting)
		{
			if (stateInfo != null)
				stateInfo.Close();

			stateInfo = MsgBox.Show("Connecting...", MsgBoxButtons.None);
			connecting = true;
		}

		prevState = State;

		if (connecting)
			return;

		if (State != CharacterSelectState.Waiting && Connection.Client.State == ConnectionState.Disconnected)
		{
			State = CharacterSelectState.Waiting;

			if (stateInfo != null)
				stateInfo.Close();

			MsgBox.Show("Failed to connect.");
		}
		else if (State == CharacterSelectState.Connecting && Connection.Client.State == ConnectionState.Connected)
		{
			State = CharacterSelectState.Login;

			var packet = new Packet(Op.ChannelLogin, 0x200000000000000F);
			packet.PutString(Connection.AccountName);
			packet.PutString(Connection.AccountName);
			packet.PutLong(Connection.SessionKey);
			packet.PutLong(selectedCharacter.EntityId);
			Connection.Client.Send(packet);
		}
	}

	private void OnCharacterSelected(long entityId)
	{
		selectedCharacter = characters.FirstOrDefault(a => a.EntityId == entityId);
		if (selectedCharacter == null)
		{
			Debug.LogErrorFormat("Missing character '{0}'.", entityId);
			return;
		}

		UpdateChannels(selectedCharacter.Server);
		SelChannel.interactable = true;

		var server = selectedCharacter.Server;
		var name = selectedCharacter.Name;
		var color = MabiMath.GetNameColor(name).ToString("X6");

		TxtSelectedChar.text = string.Format("<size=20>{0}</size>\n<color=#{2}>{1}</color>", server, name, color);
		Connection.SelectedCharacter = selectedCharacter;
	}

	private void UpdateChannels(string serverName)
	{
		SelChannel.ClearOptions();
		channels.Clear();

		var server = Connection.Servers.FirstOrDefault(a => a.Name == serverName);
		if (server == null)
			return;

		channels.AddRange(server.Channels.OrderBy(a => a.Name));

		var list = new List<string>();
		foreach (var channel in channels)
			list.Add(channel.Name + " [" + channel.State + "]");

		SelChannel.AddOptions(list);
		SelChannel_OnValueChanged(0);
	}

	public void Reset()
	{
		selectedCharacter = null;
		selectedChannel = null;
		BtnStart.interactable = false;
		SelChannel.interactable = false;
		SelChannel.ClearOptions();
	}

	private void SelChannel_OnValueChanged(int selectedIndex)
	{
		selectedChannel = channels[selectedIndex];
		BtnStart.interactable = (selectedChannel.State != ChannelState.Maintenance && selectedChannel.State < ChannelState.Bursting && !selectedCharacter.IsPartner);
	}

	private void BtnStart_OnClick()
	{
		if (selectedCharacter == null || selectedChannel == null)
		{
			Debug.LogError("Selected character or channel is null.");
			return;
		}

		State = CharacterSelectState.Connecting;

		var packet = new Packet(Op.DisconnectInform, 0);
		packet.PutString(Connection.AccountName);
		packet.PutLong(Connection.SessionKey);
		packet.PutUInt(0x800000EE);
		packet.PutUInt(0);
		packet.PutUInt(0);
		Connection.Client.Send(packet);

		packet = new Packet(Op.ChannelInfoRequest, 0);
		packet.PutString(selectedCharacter.Server);
		packet.PutString(selectedChannel.Name);
		packet.PutByte(0);
		packet.PutInt(1);
		packet.PutLong(selectedCharacter.EntityId);
		Connection.Client.Send(packet);
	}
}

public enum CharacterSelectState
{
	Waiting,
	Connecting,
	Login,
	LoggedIn,
}
