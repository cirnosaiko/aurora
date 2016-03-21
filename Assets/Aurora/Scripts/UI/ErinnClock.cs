// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ErinnClock : MonoBehaviour
{
	public bool TwentyFour = false;
	public bool ShowMinutes = false;

	private Text text;

	void Start()
	{
		text = GetComponent<Text>();
	}

	void Update()
	{
		var now = ErinnTime.Now;
		var hour = now.Hour;
		var minute = now.Minute;
		var pastNoon = (hour >= 12);
		var pastOne = (hour >= 13);

		if (!TwentyFour && pastOne)
			hour -= 12;

		if (!ShowMinutes)
			minute -= minute % 10;

		var time = string.Format("{0:00}:{1:00}", hour, minute);
		if (!TwentyFour)
			time += " " + (pastNoon ? "P.M." : "A.M.");

		text.text = time;
	}
}
