// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System;

public class CreatureController : MonoBehaviour
{
	public float RunningSpeed = 373.850600f;
	public float WalkingSpeed = 207.689200f;
	public float RotationDelay = 0.01f;

	private Transform myTransform;

	private Vector3 destination;
	private double moveDuration;
	private Vector3 movement;
	private Quaternion rotateTo;
	private Animator animator;
	private bool walking;
	private LayerMask layerMask;

	void Start()
	{
		myTransform = transform;
		destination = myTransform.position;
		rotateTo = myTransform.rotation;
		animator = GetComponentInChildren<Animator>();
		layerMask = LayerMask.GetMask("Ground");
	}

	void Update()
	{
		var pos = myTransform.position;
		var rotation = myTransform.rotation;
		var moving = false;

		// Move
		if (moveDuration != 0)
		{
			moveDuration = Math.Max(0, moveDuration - Time.deltaTime);
			pos += movement * Time.deltaTime;
			moving = true;
		}

		// Gravitate towards floor
		RaycastHit hit;
		if (Physics.Raycast(pos + Vector3.up * 1000, Vector3.down, out hit, 2000, layerMask))
			pos.y = hit.point.y;

		// Rotate
		rotation = Quaternion.Slerp(transform.rotation, rotateTo, Time.deltaTime / RotationDelay);

		// Animate
		if (animator != null)
		{
			animator.SetBool("walking", moving && walking);
			animator.SetBool("running", moving && !walking);
		}

		myTransform.position = pos;
		myTransform.rotation = rotation;

		Debug.DrawLine(pos, destination);
	}

	public void Move(Vector3 point, bool walking)
	{
		destination = point;
		this.walking = walking;

		var pos = myTransform.position;
		var speed = (walking ? WalkingSpeed : RunningSpeed) / 100f;

		var diffX = destination.x - pos.x;
		var diffZ = destination.z - pos.z;
		var distance = Math.Sqrt(diffX * diffX + diffZ * diffZ);

		var moveTime = (distance / speed);
		var moveX = (diffX / moveTime);
		var moveZ = (diffZ / moveTime);

		moveDuration = moveTime;
		movement = new Vector3((float)moveX, 0, (float)moveZ);
		rotateTo = Quaternion.LookRotation(movement);
	}
}
