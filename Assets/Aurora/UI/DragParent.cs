// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using UnityEngine.EventSystems;

public class DragParent : MonoBehaviour, IBeginDragHandler, IDragHandler
{
	private Transform parent;
	private Vector2 dragStart;

	void Start()
	{
		parent = transform.parent;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragStart = parent.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		parent.position = dragStart + (eventData.position - eventData.pressPosition);
	}
}
