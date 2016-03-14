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
			var name = character.Name;
			if (character.IsPartner)
				name += "(Partner)";
			else if (character.IsPet)
				name += "(Pet)";

			var buttonObj = GameObject.Instantiate(ButtonReference);
			buttonObj.transform.SetParent(transform);

			var text = buttonObj.GetComponentInChildren<Text>();
			text.text = name;

			var button = buttonObj.GetComponent<Button>();
			button.onClick = new Button.ButtonClickedEvent();
			button.onClick.AddListener(() => { OnCharacterSelected(character); });
		}
	}

	private void OnCharacterSelected(CharacterInfo character)
	{
		Debug.Log(character.Name + " : " + character.EntityId);
	}
}
