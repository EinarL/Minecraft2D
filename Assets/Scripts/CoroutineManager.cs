using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
	/**
	 * enqueue a coroutine to this queue so that it will be executed.
	 * this is neccessary so a worker thread can tell the main thread to execute a coroutine.
	 */
	private ConcurrentQueue<IEnumerator> coroutineQueue = new ConcurrentQueue<IEnumerator>();

	void Start()
	{
		StartCoroutine(processQueue());
	}

	private IEnumerator processQueue()
	{
		while (true)
		{
			while (coroutineQueue.TryDequeue(out IEnumerator coroutine))
			{
				StartCoroutine(coroutine);
			}
			yield return new WaitForSeconds(0.5f);
		}

	}

	public void enqueueCoroutine(IEnumerator coroutine)
	{
		coroutineQueue.Enqueue(coroutine);
	}


	public void startCoroutine(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}
}
