using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MsgBox : MonoBehaviour, IBeginDragHandler, IDragHandler
{
	public MsgBoxResult Result;

	private Vector2 dragStart;

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

		txtMessage.text = text;

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

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragStart = transform.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		transform.position = dragStart + (eventData.position - eventData.pressPosition);
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
