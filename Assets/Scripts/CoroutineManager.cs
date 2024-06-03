using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
	public void startCoroutine(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}
}
