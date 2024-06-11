using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveChunk
{
	private static string path = Application.persistentDataPath + "\\worlds\\devWorld\\chunk_data";

	/**
	 * Save chunk data 
	 */
	public static void save(ChunkData chunkData)
	{
		// if directory doesnt exist, the create it
		if (!Directory.Exists(path))
		{
			//Debug.Log("creating directory: " + path);
			Directory.CreateDirectory(path);
		}

		BinaryFormatter formatter = new BinaryFormatter();

		string filePath = path + "\\chunk" + chunkData.getChunkPosition() + ".file";
		FileStream stream = new FileStream(filePath, FileMode.Create);

		//Debug.Log("saving chunk at " + filePath);

		formatter.Serialize(stream, chunkData); // write chunkData to stream
		stream.Close();
	}

	/**
	 * Load chunk data
	 */
	public static ChunkData load(int chunkPosition)
	{
		string filePath = path + "\\chunk" + chunkPosition + ".file";
		if (File.Exists(filePath))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(filePath, FileMode.Open);

			ChunkData data = formatter.Deserialize(stream) as ChunkData;
			stream.Close();

			return data;
		}
		else
		{
			Debug.LogError("Chunk file not found in " + filePath);
			return null;
		}
	}

	/**
	 * returns true if a file for this chunk position exists, otherwise false
	 */
	public static bool exists(int chunkPosition)
	{
		return File.Exists(path + "\\chunk" + chunkPosition + ".file");
	}

	public static void setWorldFolder(string worldName)
	{
		path = path.Replace("devWorld", worldName);
	}
}
