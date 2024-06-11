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
 * currently saves the inventory and furnaces & more.
 */
public class JsonDataService : IDataService
{
	private static readonly object myLock = new object ();

	private string folderPath = "\\worlds\\devWorld\\general\\"; // We'll replace the devWorld by the world name

	private static JsonDataService instance = null;

	public static JsonDataService Instance
	{
		get
		{
			lock (myLock)
				{
					if (instance == null)
					{
						instance = new JsonDataService();
					}
					return instance;
				}
		}
	}


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

	/**
	 * this currently can only add to tombstone data which looks like [[xPos, yPos, inv], [xPos,yPos, inv], ...]
	 * if i need this implementation for other things then i should probably change this to an abstract class and have this function be virtual.
	 * so that i can make different implementations for this function.
	 */
	public bool appendToData(string filename, object[] data)
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
				List<object[]> dataContent = loadData<List<object[]>>(filename);

				dataContent.Add(data);

				File.WriteAllText(path, JsonConvert.SerializeObject(dataContent));
			}
			else
			{
				using FileStream stream = File.Create(path);
				stream.Close();
				File.WriteAllText(path, JsonConvert.SerializeObject(new object[] { data }));
			}

			return true;
		}
		catch (Exception e)
		{
			Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
			return false;
		}
	}

	/**
	 * i should create a subclass for these tombstone functions.
	 */
	public void removeTombstoneData(float xPos, float yPos)
	{
		string path = Application.persistentDataPath + folderPath + "tombstone.json";
		// if directory doesnt exist, the create it
		if (!Directory.Exists(Application.persistentDataPath + folderPath))
		{
			Directory.CreateDirectory(Application.persistentDataPath + folderPath);
		}
		try
		{
			if (File.Exists(path))
			{
				List<object[]> dataContent = loadData<List<object[]>>("tombstone.json");

				List<object[]> replacementData = new List<object[]>();
				foreach (object[] tombstone in dataContent)
				{
					if (!((Double)tombstone[0] == xPos && (Double)tombstone[1] == yPos)) // if this is not the tombstone we want to remove
					{
						replacementData.Add(tombstone);
					}
				}

				File.WriteAllText(path, JsonConvert.SerializeObject(replacementData));
			}
			else
			{
				Debug.LogError("tombstone.json file doesnt exist!");
			}

		}
		catch (Exception e)
		{
			Debug.LogError($"Unable to edit data due to: {e.Message} {e.StackTrace}");
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


	public void setWorldFolder(string worldName)
	{
		folderPath = folderPath.Replace("devWorld", worldName);
	}

}
