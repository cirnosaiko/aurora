// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Aura.Mabi;
using Aura.Mabi.Network;

public class Chat : MonoBehaviour
{
	public int MaxMessages = 20;
	public int MaxLogMessages = 100;

	public Transform Messages;
	public InputField TxtChat;
	public GameObject ReferenceMessage;

	public Transform WndChatLog;
	public Transform WndChatLogMessages;
	public ScrollRect WndChatScrollView;

	private Image background;
	private int scrollDown;

	void Start()
	{
		background = GetComponent<Image>();
		background.enabled = false;
	}

	void Update()
	{
		// Scroll down over multiple frames, to makes sure it actually does
		// scroll all the way down. No matter what I did, the last message
		// were always hidden...
		if (scrollDown != 0)
		{
			scrollDown = Mathf.Max(0, scrollDown - 1);
			WndChatScrollView.verticalNormalizedPosition = 0;
		}

		// Toggle input
		if (Input.GetKeyDown(KeyCode.Return))
		{
			TxtChat.gameObject.SetActive(!TxtChat.gameObject.activeSelf);
			if (TxtChat.gameObject.activeSelf)
				TxtChat.OnPointerClick(new PointerEventData(EventSystem.current));
			else
				// Select nothing, so the input field doesn't stay selected,
				// preventing the log toggle.
				EventSystem.current.SetSelectedGameObject(null);
		}

		// Toggle background gradient
		if (background.enabled)
		{
			if (Messages.childCount == 0 && !TxtChat.gameObject.activeSelf)
				background.enabled = false;
		}
		else
		{
			if (Messages.childCount != 0 || TxtChat.gameObject.activeSelf)
				background.enabled = true;
		}

		// Toggle log if no input field is selected
		if (Input.GetKeyDown(KeyCode.L))
		{
			var toggle = false;
			var obj = EventSystem.current.currentSelectedGameObject;
			if (obj == null)
			{
				toggle = true;
			}
			else
			{
				var component = obj.GetComponent<InputField>();
				if (component == null)
					toggle = true;
			}

			if (toggle)
				WndChatLog.gameObject.SetActive(!WndChatLog.gameObject.activeSelf);
		}
	}

	void OnGUI()
	{
		// Submit message, done here because isFocused doesn't work properly
		// from Update.
		var text = TxtChat.text.TrimEnd();
		if (Input.GetKeyDown(KeyCode.Return) && TxtChat.isFocused && text != "")
		{
			if (Connection.Client.State == ConnectionState.Connected)
			{
				var packet = new Packet(Op.Chat, Connection.ControllingEntityId);
				packet.PutByte(1);
				packet.PutString(text);
				Connection.Client.Send(packet);
			}
			// Offline test
			else
			{
				AddMessage("test", text);
			}

			TxtChat.text = "";
		}
	}

	public void AddMessage(string name, string message)
	{
		var color = MabiMath.GetNameColor(name);
		AddMessage(color, name, message);
	}

	public void AddMessage(int color, string name, string message)
	{
		var msgObj = GameObject.Instantiate(ReferenceMessage);
		var text = msgObj.GetComponent<Text>();

		// Text
		var str = string.Format("{0}: {1}", name, message, color);
		text.text = str;

		// Coloring the text instead of using rich text because RT-colored
		// text can't be faded.
		byte r = (byte)((color >> 16) & 0xFF);
		byte g = (byte)((color >> 8) & 0xFF);
		byte b = (byte)((color) & 0xFF);
		text.color = new Color32(r, g, b, 255);

		msgObj.transform.SetParent(Messages);
		LimitChildren(Messages, MaxMessages);

		// Log
		var msgObj2 = GameObject.Instantiate(msgObj);
		msgObj2.GetComponent<FadeAndDestroyTextAfterTime>().enabled = false;
		msgObj2.transform.SetParent(WndChatLogMessages);
		LimitChildren(WndChatLogMessages, MaxLogMessages);
		scrollDown = 3;
	}

	private void LimitChildren(Transform transform, int max)
	{
		var remove = Mathf.Max(0, transform.childCount - max);
		if (remove == 0)
			return;

		for (int i = 0; i < remove; ++i)
		{
			var child = transform.GetChild(0);
			GameObject.Destroy(child.gameObject);
		}
	}

	public void OpenLog()
	{
		WndChatLog.gameObject.SetActive(true);
	}

	public void CloseLog()
	{
		WndChatLog.gameObject.SetActive(false);
	}
}
