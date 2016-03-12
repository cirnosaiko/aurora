// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginForm : MonoBehaviour
{
	public InputField TxtUsername;
	public InputField TxtPassword;

	public void BtnLogin_OnClick()
	{
		Debug.Log("BtnLogin_OnClick");
		Debug.Log("Username: " + TxtUsername.text);
		Debug.Log("Password: " + TxtPassword.text);
	}
}
