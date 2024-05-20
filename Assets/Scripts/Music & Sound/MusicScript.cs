using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * this script is responsible for playing music and ambient cave noises
 */
public class MusicScript : MonoBehaviour
{
	private AudioSource audioSource;
	private AudioClip[] overworldMusic;
	private AudioClip[] caveAmbientNoise;

	private int playMusicChance = 10;
	private int playCaveNoiseChance = 5;

	// Start is called before the first frame update
	void Start()
    {
		audioSource = GameObject.Find("Audio").transform.Find("MusicAndAmbient").GetComponent<AudioSource>();
		overworldMusic = Resources.LoadAll<AudioClip>("Sounds\\Music\\Overworld");
		caveAmbientNoise = Resources.LoadAll<AudioClip>("Sounds\\Ambient\\Cave");
		StartCoroutine(decideIfPlayMusic());
		StartCoroutine(decideIfPlayCaveAmbientNoise());
	}


	private IEnumerator decideIfPlayMusic()
	{
		while (true)
		{
			yield return new WaitForSeconds(60f);
			float rand = Random.value * 100;
			if (rand < playMusicChance && !audioSource.isPlaying)
			{
				var random = new System.Random();
				int randIndex = random.Next(overworldMusic.Length);
				AudioClip randClip = overworldMusic[randIndex];
				audioSource.clip = randClip;
				audioSource.Play();
			}
		}
	}

	private IEnumerator decideIfPlayCaveAmbientNoise()
	{
		while (true)
		{
			yield return new WaitForSeconds(80f);
			float rand = Random.value;
			if (rand < playCaveNoiseChance && !audioSource.isPlaying && transform.position.y < -30)
			{
				var random = new System.Random();
				int randIndex = random.Next(caveAmbientNoise.Length);
				AudioClip randClip = caveAmbientNoise[randIndex];
				audioSource.clip = randClip;
				audioSource.Play();
			}
		}
	}
}
