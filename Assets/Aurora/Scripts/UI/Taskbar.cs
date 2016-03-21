// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using UnityEngine.UI;

public class Taskbar : MonoBehaviour
{
	public Button StartButton;
	public Transform StartMenu;

	private HoverCheck startMenuHoverCheck;
	private MsgBox endGameMsg;

	void Start()
	{
		StartMenu.gameObject.SetActive(false);
		startMenuHoverCheck = StartMenu.GetComponent<HoverCheck>();
	}

	void Update()
	{
		// End Game msgbox
		if (endGameMsg != null && endGameMsg.Result != MsgBoxResult.Pending)
		{
			if (endGameMsg.Result == MsgBoxResult.Yes)
				Application.Quit();

			endGameMsg = null;
		}

		// Make taskbar disappear if clicked elsewhere
		if (StartMenu.gameObject.activeSelf && Input.GetMouseButtonDown(0) && !startMenuHoverCheck.IsOver)
			HideStartMenu();
	}

	public void ShowStartMenu()
	{
		StartMenu.gameObject.SetActive(true);
	}

	public void HideStartMenu()
	{
		StartMenu.gameObject.SetActive(false);
	}

	public void ToggleStartMenu()
	{
		StartMenu.gameObject.SetActive(!StartMenu.gameObject.activeSelf);
	}

	public void EndGame()
	{
		if (endGameMsg == null)
			endGameMsg = MsgBox.Show("Do you want to end the game?", MsgBoxButtons.YesNo);
		HideStartMenu();
	}
}
