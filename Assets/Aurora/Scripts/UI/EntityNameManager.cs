// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityNameManager : MonoBehaviour
{
	public Transform EntityList;

	private Transform me;
	private Camera mainCamera;
	private GameObject reference;
	private Dictionary<Transform, Transform> nameTransforms = new Dictionary<Transform, Transform>();

	void Start()
	{
		me = transform;
		mainCamera = Camera.main;
		reference = me.FindChild("TxtReference").gameObject;
	}

	void Update()
	{
		// Go through all entities
		foreach (Transform entityTransform in EntityList)
		{
			// Get entity info component, ignore empty names
			var entityInfo = entityTransform.GetComponent<EntityInfo>();
			if (string.IsNullOrEmpty(entityInfo.Name))
				continue;

			// Create new name object if new entity
			Transform nameTransform;
			if (!nameTransforms.TryGetValue(entityTransform, out nameTransform))
			{
				// Create object in list
				var newNameObj = GameObject.Instantiate(reference);
				nameTransform = newNameObj.transform;
				nameTransform.SetParent(me);

				// Set colored name
				var color = MabiMath.GetNameColor(entityInfo.Name);
				var text = nameTransform.GetComponent<Text>();
				var name = entityInfo.Name;
				text.text = string.Format("<color=#{0:X6}>{1}</color>", color, name);

				// Save for automatic removal
				nameTransforms.Add(entityTransform, nameTransform);
			}

			// Get information for placement
			var nameObject = nameTransform.gameObject;
			var nameTarget = entityTransform.FindChild("NameTarget");
			var pos = mainCamera.WorldToScreenPoint(nameTarget.transform.position);

			// If entity is visible, activate name object and move it into
			// position.
			if (pos.z > 0)
			{
				if (!nameObject.activeSelf)
					nameObject.SetActive(true);
				nameTransform.position = pos;
			}
			// If entity is not visible deactivate the name.
			else if (nameObject.activeSelf)
				nameObject.SetActive(false);
		}

		// Check for removed entities
		var toRemove = new List<Transform>();
		foreach (var nameTransformX in nameTransforms)
		{
			// Remove name if entity transform became null (destroyed)
			var entityTransform = nameTransformX.Key;
			if (entityTransform == null)
			{
				var nameTransform = nameTransformX.Value;

				GameObject.Destroy(nameTransform);
				toRemove.Add(entityTransform);
			}
		}

		// Remove destroyed entity's names from the list
		if (toRemove.Count != 0)
		{
			foreach (var key in toRemove)
				nameTransforms.Remove(key);
		}
	}
}
