// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;

public class CursorSwap : MonoBehaviour
{
	public LayerMask LayerMask;
	public Texture2D Normal;
	public Texture2D Talk;

	private Texture2D current;

	void Start()
	{
		current = Normal;
	}

	void Update()
	{
		var set = Normal;

		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 2000, LayerMask))
		{
			var hitTransform = hit.transform;
			var entityInfo = hitTransform.GetComponent<EntityInfo>();
			if (entityInfo != null)
			{
				if (entityInfo.IsConversationNpc)
					set = Talk;
			}
		}

		if (set != current)
		{
			current = set;
			Cursor.SetCursor(set, new Vector2(0, 0), CursorMode.Auto);
		}
	}
}
