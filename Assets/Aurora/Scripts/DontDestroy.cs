// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;

public class DontDestroy : MonoBehaviour
{
	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
}
