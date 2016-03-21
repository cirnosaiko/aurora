// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Aura.Mabi.Network;

public class RegionManager : MonoBehaviour
{
	public int MinLoadingTime = 0;

	private AsyncOperation loading;
	private float loadStart;

	void Update()
	{
		if (loading == null)
			return;

		if (loadStart + MinLoadingTime <= Time.time)
			loading.allowSceneActivation = true;

		if (!loading.isDone)
			return;

		loading = null;

		var packet = new Packet(Op.EnterRegionRequest, Connection.SelectedCharacter.EntityId);
		Connection.Client.Send(packet);
	}

	public void Load(string sceneName)
	{
		StartCoroutine("LoadStart", sceneName);
	}

	private IEnumerator LoadStart(string sceneName)
	{
		SceneManager.LoadScene("Loading");
		yield return null;

		loadStart = Time.time;
		loading = SceneManager.LoadSceneAsync(sceneName);
		loading.allowSceneActivation = false;
	}

	public static bool Load(int regionId)
	{
		string name;
		switch (regionId)
		{
			case 1: name = "Uladh_main"; break;

			default:
				Debug.Log("Missing region: " + regionId);
				return false;
		}

		var system = GameObject.Find("System");
		var manager = system.GetComponent<RegionManager>();
		manager.Load(name);

		return true;
	}
}
