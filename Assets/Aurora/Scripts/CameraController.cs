// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public float RotationSpeed = 360;
	public float MinTilde = -30;
	public float MaxTilde = 60;
	public float MinZoom = 2;
	public float MaxZoom = 15;
	public float ZoomSpeed = 20;

	private Transform cameraTransform;
	private Transform target;
	private float rotationX = 0;
	private float rotationY = 30;
	private float distance = 7;

	void Start()
	{
		cameraTransform = Camera.main.transform;
		SetTarget(GameObject.FindGameObjectWithTag("Player"));
	}

	void Update()
	{
		if (target == null)
			return;

		if (Input.GetMouseButton(1))
		{
			rotationX += Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime;
			rotationY -= Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, MinTilde, MaxTilde);
		}

		if (Input.GetAxis("Mouse ScrollWheel") > 0 && distance > MinZoom)
			distance -= ZoomSpeed * Time.deltaTime;
		else if (Input.GetAxis("Mouse ScrollWheel") < 0 && distance < MaxZoom)
			distance += ZoomSpeed * Time.deltaTime;

		var rotation = Quaternion.Euler(rotationY, rotationX, 0);
		cameraTransform.rotation = rotation;
		cameraTransform.position = rotation * new Vector3(0, 0, -distance) + target.position;
	}

	public void SetTarget(GameObject target)
	{
		SetTarget(target == null ? null : target.transform);
	}

	public void SetTarget(Transform target)
	{
		if (target != null)
		{
			if (target.name != "CameraTarget")
			{
				var ct = target.FindChild("CameraTarget");
				if (ct != null)
					target = ct;
			}
		}

		this.target = target;
	}
}
