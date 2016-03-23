// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ErinnClock : MonoBehaviour
{
	public bool twentyFour = false;
	public bool showMinutes = false;

	// Properties allow dynamic setting from events in the editor,
	// but fields are the only thing shown in the script options in the editor.
	public bool TwentyFour { get { return twentyFour; } set { twentyFour = value; } }
	public bool ShowMinutes { get { return showMinutes; } set { showMinutes = value; } }

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

		if (!twentyFour && pastOne)
			hour -= 12;

		if (!showMinutes)
			minute -= minute % 10;

		var time = string.Format("{0:00}:{1:00}", hour, minute);
		if (!twentyFour)
			time += " " + (pastNoon ? "P.M." : "A.M.");

		text.text = time;
	}
}
