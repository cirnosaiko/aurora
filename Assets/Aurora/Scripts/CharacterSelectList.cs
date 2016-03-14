// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class CharacterSelectList : MonoBehaviour
{
	public GameObject ButtonReference;

	private CharacterInfo[] characters;

	void Start()
	{
		var transform = this.transform;

		characters = Connection.Characters.OrderBy(a => a.EntityId).ToArray();
		foreach (var character in characters)
		{
			var entityId = character.EntityId;
			var name = character.Name;
			if (character.IsPartner)
				name += " (Partner)";
			else if (character.IsPet)
				name += " (Pet)";

			var buttonObj = GameObject.Instantiate(ButtonReference);
			buttonObj.transform.SetParent(transform);

			var text = buttonObj.GetComponentInChildren<Text>();
			text.text = name;

			var button = buttonObj.GetComponent<Button>();
			button.onClick.AddListener(() => { OnCharacterSelected(entityId); });
		}
	}

	private void OnCharacterSelected(long entityId)
	{
		var character = characters.FirstOrDefault(a => a.EntityId == entityId);
		if (character == null)
		{
			Debug.LogErrorFormat("Missing character '{0}'.", entityId);
			return;
		}

		if (character.IsPartner)
		{
			MsgBox.Show("You can't use a Partner to login.");
			return;
		}

		Debug.LogFormat("{0} : {1:X16}", character.Name, character.EntityId);
	}
}
