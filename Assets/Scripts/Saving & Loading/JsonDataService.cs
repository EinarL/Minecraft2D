using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


/**
 * this class is used to save data to json format.
 * currently saves the inventory and furnaces.
 */
public class JsonDataService : IDataService
{
	private string folderPath = "\\general\\";

	public bool saveData<T>(string filename, T data)
	{
		string path = Application.persistentDataPath + folderPath + filename;
		// if directory doesnt exist, the create it
		if (!Directory.Exists(Application.persistentDataPath + folderPath))
		{
			Directory.CreateDirectory(Application.persistentDataPath + folderPath);
		}
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			using FileStream stream = File.Create(path);
			stream.Close();
			File.WriteAllText(path, JsonConvert.SerializeObject(data));
			return true;
		}
		catch(Exception e)
		{
			Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
			return false;
		}
	}

	public T loadData<T>(string filename)
	{
		string path = Application.persistentDataPath + folderPath + filename;
		if(!File.Exists(path))
		{
			throw new FileNotFoundException($"{path} does not exists");
		}

		try
		{
			T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
			return data;
		}
		catch(Exception e)
		{
			Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
			throw e;
		}
	}

	public bool exists(string filename)
	{
		return File.Exists(Application.persistentDataPath + folderPath + filename);
	}

}
