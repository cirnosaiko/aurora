// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
			var name = string.Format("{0} ({1})", character.Name, character.Server);

			var buttonObj = GameObject.Instantiate(ButtonReference);
			buttonObj.transform.SetParent(transform);

			var text = buttonObj.GetComponentInChildren<Text>();
			text.text = name;

			var disable = buttonObj.transform.FindChild("Img" + (character.IsPet ? "Character" : "Pet"));
			disable.gameObject.SetActive(false);

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

		MsgBox.Show("TODO: Login with " + character.Name + ".");
	}
}
