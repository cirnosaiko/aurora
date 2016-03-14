// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class LoginForm : MonoBehaviour
{
	public GameObject FrmLogin;
	public InputField TxtUsername;
	public InputField TxtPassword;

	[Range(1, 30)]
	public int ConnectionTimeout = 2;

	private bool connecting;
	private float connectionTimeout;
	private MsgBox processInfo;

	void Update()
	{
		if (connecting && connectionTimeout <= Time.time)
		{
			connecting = false;
			processInfo.Close();
			FrmLogin.SetActive(true);
			MsgBox.Show("Connection failed.");
		}
	}

	public void BtnLogin_OnClick()
	{
		var username = TxtUsername.text;
		var password = TxtPassword.text;

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			return;

		connecting = true;
		connectionTimeout = Time.time + ConnectionTimeout;
		processInfo = MsgBox.Show("Connecting...", MsgBoxButtons.None);

		FrmLogin.SetActive(false);
	}
}
