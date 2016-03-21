// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FadeAndDestroyTextAfterTime : MonoBehaviour
{
	public float DestroyDelay = 10;
	public float StartFadeAt = 2f;

	private Text text;

	void OnValidate()
	{
		DestroyDelay = Mathf.Max(1, DestroyDelay);
		StartFadeAt = Mathf.Clamp(StartFadeAt, 0.5f, DestroyDelay);
	}

	void Start()
	{
		text = GetComponent<Text>();
	}

	void Update()
	{
		if (DestroyDelay <= StartFadeAt)
		{
			var color = text.color;
			color.a = (1f / StartFadeAt * DestroyDelay);
			text.color = color;
		}

		DestroyDelay = Mathf.Max(0, DestroyDelay - Time.deltaTime);
		if (DestroyDelay == 0)
			GameObject.Destroy(gameObject);
	}
}
