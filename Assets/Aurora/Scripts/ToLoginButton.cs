// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using UnityEngine.SceneManagement;

public class ToLoginButton : MonoBehaviour
{
	public void OnClick()
	{
		Connection.Reset();
		SceneManager.LoadScene("Login");
	}
}
