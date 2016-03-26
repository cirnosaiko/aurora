// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Aura.Mabi.Network;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using Aura.Mabi.Const;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LoginForm : MonoBehaviour
{
	public GameObject Elements;
	public InputField TxtUsername;
	public InputField TxtPassword;
	public Button BtnLogin;
	public Button BtnEnd;

	public string Host = "127.0.0.1";
	public int Port = 11000;

	[HideInInspector]
	public LoginState State;

	private LoginState prevState;
	private MsgBox stateInfo;
	private MsgBox endQuery;

	void Start()
	{
		BtnLogin.onClick.AddListener(BtnLogin_OnClick);
		BtnEnd.onClick.AddListener(BtnEnd_OnClick);

		var args = Environment.GetCommandLineArgs();
		if (args.Length > 1)
		{
			foreach (var arg in args)
			{
				var split = arg.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (split.Length != 2)
					continue;

				var key = split[0];
				var value = split[1];

				switch (key)
				{
					case "logip": Host = value; break;
					case "logport":
						try
						{
							Port = Convert.ToInt32(value);
						}
						catch (Exception)
						{
							Debug.LogError("Failed to parse logport argument.");
						}
						break;
				}
			}
		}
	}

	void Update()
	{
		// Submit
		if (Input.GetKeyDown(KeyCode.Return))
		{
			var system = EventSystem.current;
			var selected = system.currentSelectedGameObject;

			if (selected == TxtUsername.gameObject || selected == TxtPassword.gameObject)
			{
				BtnLogin.OnPointerClick(new PointerEventData(system));
				return;
			}
		}

		// End Game msg box
		if (endQuery != null && endQuery.Result != MsgBoxResult.Pending)
		{
			if (endQuery.Result == MsgBoxResult.Yes)
			{
				Application.Quit();
				return;
			}

			endQuery.Close();
			endQuery = null;
		}

		if (prevState != State)
		{
			string message = null;
			switch (State)
			{
				case LoginState.Connecting: message = "Connecting..."; break;
				case LoginState.Login: message = "Logging in..."; break;
			}

			if (message != null)
			{
				if (stateInfo != null)
					stateInfo.Close();

				stateInfo = MsgBox.Show(message, MsgBoxButtons.None);
			}
		}

		prevState = State;

		// Nothing to do if we're waiting for input
		if (State == LoginState.Waiting)
			return;

		// Connection loss
		if (Connection.Client.State == ConnectionState.Disconnected)
		{
			string message = null;
			switch (State)
			{
				case LoginState.Connecting: message = "Connection failed."; break;
				case LoginState.Ident: message = "Disconnected during identification."; break;
				case LoginState.Login: message = "Disconnected during login."; break;
				default: message = "Disconnected."; break;
			}

			ResetForm(message);
			return;
		}

		// Go from connecting to ident, afterwards the packets take over
		if (State == LoginState.Connecting && Connection.Client.State == ConnectionState.Connected)
		{
			var packet = new Packet(Op.ClientIdent, 0);
			packet.PutByte(true);
			packet.PutString("Aura-Aurora-Dev");
			packet.PutInt(0);
			packet.PutString("");
			packet.PutString("");
			Connection.Client.Send(packet);

			State = LoginState.Ident;
			return;
		}
	}

	public void ResetForm(string message)
	{
		State = LoginState.Waiting;
		Elements.SetActive(true);

		if (stateInfo != null)
			stateInfo.Close();

		if (message != null)
			MsgBox.Show(message);
	}

	public void BtnLogin_OnClick()
	{
		var username = TxtUsername.text;
		var password = TxtPassword.text;

		if (username.Length < 4 || password.Length < 4)
		{
			MsgBox.Show("Your username and password must have at least 4 characters.");
			return;
		}

		State = LoginState.Connecting;
		Connection.Client.ConnectAsync(Host, Port);

		Elements.SetActive(false);
	}

	public void BtnEnd_OnClick()
	{
		if (endQuery != null)
			return;

		endQuery = MsgBox.Show("Do you want to end the game?", MsgBoxButtons.YesNo);
	}
}

public enum LoginState
{
	Waiting,
	Connecting,
	Ident,
	Login,
	LoggedIn,
}
