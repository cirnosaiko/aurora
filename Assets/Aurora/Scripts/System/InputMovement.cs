// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Aura.Mabi.Network;
using System;

public class InputMovement : MonoBehaviour
{
	public float MoveDelay = 0.25f;

	private GameObject target;
	private float moveDelayTimeout = 0;

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

		if (moveDelayTimeout != 0)
			moveDelayTimeout = Math.Max(0, moveDelayTimeout - Time.deltaTime);

		if (moveDelayTimeout == 0 && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				var shift = Input.GetKey(KeyCode.LeftShift);
				//var controller = target.GetComponent<CreatureController>();
				//controller.Move(hit.point, shift);

				var x = (int)(hit.point.x * 100);
				var y = (int)(hit.point.z * 100);

				var packet = new Packet(shift ? Op.Walk : Op.Run, Connection.ControllingEntityId);
				packet.PutInt(x);
				packet.PutInt(y);
				packet.PutByte(1);
				packet.PutByte(0);
				Connection.Client.Send(packet);

				moveDelayTimeout = MoveDelay;
			}
		}
	}
}
