// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InputMovement : MonoBehaviour
{
	void Start()
	{

	}

	void Update()
	{
		if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				var player = GameObject.FindGameObjectWithTag("Player");
				var controller = player.GetComponent<CreatureController>();
				controller.Move(hit.point, false);
			}
		}
	}
}
