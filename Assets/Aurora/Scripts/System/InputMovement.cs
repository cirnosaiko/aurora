// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InputMovement : MonoBehaviour
{
	private GameObject target;

	void Start()
	{
		var player = GameObject.FindGameObjectWithTag("Player");
		SetTarget(player);
	}

	void OnLevelWasLoaded(int level)
	{
		Start();
	}

	public void SetTarget(GameObject target)
	{
		this.target = target;
	}

	void Update()
	{
		if (target == null)
			return;

		if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				var controller = target.GetComponent<CreatureController>();
				controller.Move(hit.point, false);
			}
		}
	}
}
