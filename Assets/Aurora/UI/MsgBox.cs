using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MsgBox : MonoBehaviour, IBeginDragHandler, IDragHandler
{
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
		var btnOkay = transform.FindChild("BtnOkay").GetComponent<Button>();

		if ((buttons & MsgBoxButtons.Okay) == 0)
			btnOkay.gameObject.SetActive(false);

		transform.SetParent(canvas.transform);
		transform.position = new Vector3(Screen.width / 2, Screen.height / 2);

		txtMessage.text = text;

		return obj.GetComponent<MsgBox>();
	}

	public void Close()
	{
		Destroy(gameObject);
	}

	public void BtnOkay_OnClick()
	{
		Close();
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
	None = 0,
	Okay = 1,
}
