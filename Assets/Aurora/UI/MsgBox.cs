using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MsgBox : MonoBehaviour
{
	public MsgBoxResult Result;

	public static MsgBox Show(string text)
	{
		return Show(text, MsgBoxButtons.Okay);
	}

	public static MsgBox Show(string text, MsgBoxButtons buttons)
	{
		var canvas = GameObject.Find("Canvas");
		if (canvas == null)
		{
			Debug.LogError("MsgBox: Canvas not found.");
			return null;
		}

		var obj = Instantiate(Resources.Load("MsgBox")) as GameObject;

		var transform = obj.transform as RectTransform;
		var txtMessage = transform.FindChild("TxtMessage").GetComponent<Text>();
		var buttonsTransform = transform.FindChild("Buttons").transform;
		var hasButtons = (buttons != MsgBoxButtons.None);

		if (!hasButtons)
		{
			foreach (Transform child in buttonsTransform)
				child.gameObject.SetActive(false);
		}
		else
		{
			foreach (var msgBoxButton in new[] { MsgBoxButtons.Okay, MsgBoxButtons.Yes, MsgBoxButtons.No })
			{
				var buttonTransform = buttonsTransform.FindChild("Btn" + msgBoxButton);
				if (buttonTransform == null)
				{
					Debug.LogErrorFormat("MsgBox: Button '{0}' not found.", msgBoxButton);
					continue;
				}

				var button = buttonTransform.GetComponent<Button>();
				if ((buttons & msgBoxButton) == 0)
					button.gameObject.SetActive(false);
			}
		}

		txtMessage.text = text;

		var width = Math.Max(200, txtMessage.preferredWidth + 20);
		var height = Math.Max(102, txtMessage.preferredHeight + 20 + 30 + (hasButtons ? 35 : 0));

		transform.sizeDelta = new Vector2(width, height);
		transform.position = new Vector3(Screen.width / 2, Screen.height / 2);
		transform.SetParent(canvas.transform);

		return obj.GetComponent<MsgBox>();
	}

	public void Close()
	{
		Close((int)MsgBoxResult.None);
	}

	public void Close(int result)
	{
		Result = (MsgBoxResult)result;
		Destroy(gameObject);
	}
}

public enum MsgBoxButtons
{
	None = 0x00,
	Okay = 0x01,
	Yes = 0x02,
	No = 0x04,

	YesNo = Yes | No,
}

public enum MsgBoxResult
{
	Pending = 0,
	None = 1,
	Okay = 2,
	Yes = 3,
	No = 4,
}
