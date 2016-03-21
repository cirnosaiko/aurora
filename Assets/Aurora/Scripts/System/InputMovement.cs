// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Aura.Mabi.Network;

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
			}
		}
	}
}
