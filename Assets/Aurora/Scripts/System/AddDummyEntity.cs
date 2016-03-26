using UnityEngine;
using System.Collections;

public class AddDummyEntity : MonoBehaviour
{
	public GameObject Dummy;

	void Start()
	{
		var parentObj = GameObject.Find("Entities");
		var terrainObj = GameObject.Find("Terrain");

		if (parentObj != null && terrainObj != null)
		{
			var terrain = terrainObj.GetComponent<Terrain>();
			var x = terrain.terrainData.size.x / 2;
			var z = terrain.terrainData.size.z / 2;

			var obj = (GameObject)GameObject.Instantiate(Dummy, new Vector3(x, 0, z), Quaternion.identity);
			obj.transform.SetParent(parentObj.transform);
			obj.layer = 8; // Entities
		}
	}
}
