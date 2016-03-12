using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MsgBox : MonoBehaviour, IBeginDragHandler, IDragHandler
{
	private Vector2 dragStart;

	public static void Show(string text)
	{
		var canvas = GameObject.Find("Canvas");
		if (canvas == null)
		{
			Debug.LogError("MsgBox: Canvas not found.");
			return;
		}

		var obj = Instantiate(Resources.Load("MsgBox")) as GameObject;

		var transform = obj.transform as RectTransform;
		var txtMessage = obj.transform.FindChild("TxtMessage").GetComponent<Text>();

		transform.SetParent(canvas.transform);
		transform.position = new Vector3(Screen.width / 2, Screen.height / 2);

		txtMessage.text = text;
	}

	public void BtnOkay_OnClick()
	{
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
