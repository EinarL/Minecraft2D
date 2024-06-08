using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WorldVariableScript : MonoBehaviour
{
	private string worldName = "";

	public void setWorldName(string worldName)
	{
		this.worldName = worldName;

		if (!Directory.Exists(Application.persistentDataPath + "\\worlds\\")) // create worlds folder if it doesn't exist
		{
			Directory.CreateDirectory(Application.persistentDataPath + "\\worlds\\");
		}
		Directory.CreateDirectory(Application.persistentDataPath + "\\worlds\\" + worldName + "\\"); // create folder for the world

		JsonDataService.Instance.setWorldFolder(worldName);
		SaveChunk.setWorldFolder(worldName);
	}

	public string getWorldName()
	{
		return worldName;
	}
}
