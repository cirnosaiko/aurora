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
		Debug.Log("BtnLogin_OnClick");
		Debug.Log("Username: " + TxtUsername.text);
		Debug.Log("Password: " + TxtPassword.text);

		connecting = true;
		connectionTimeout = Time.time + 2;
		processInfo = MsgBox.Show("Connecting...", MsgBoxButtons.None);

		FrmLogin.SetActive(false);
	}
}
