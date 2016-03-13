// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BgmManager : MonoBehaviour
{
	[Serializable]
	public class Track
	{
		public string name;
		public AudioClip clip;
	}

	public AudioClip Default;
	public Track[] Tracks;

	private AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.loop = true;

		if (Default != null)
			this.Play(Default);
	}

	void OnLevelWasLoaded(int level)
	{
		var name = SceneManager.GetActiveScene().name;
		if (TrackExists(name))
			this.Play(name);
	}

	private bool TrackExists(string name)
	{
		return Tracks.Any(a => a.name == name);
	}

	public void Play(string name)
	{
		var song = Tracks.FirstOrDefault(a => a.name == name);
		if (song == null)
		{
			Debug.LogErrorFormat("BgmManager.Play: Track '{0}' not found.", name);
			return;
		}

		Play(song.clip);
	}

	public void Play(AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.Play();
	}
}
