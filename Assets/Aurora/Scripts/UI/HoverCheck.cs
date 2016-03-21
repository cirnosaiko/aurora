// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using UnityEngine.EventSystems;

public class HoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public bool IsOver = false;

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsOver = false;
	}
}
