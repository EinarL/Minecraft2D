using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

/**
 * worker threads use this class to delegate work to the main thread.
 * this class is also used by random classes that do not extend MonoBehavior so they need to use the startCoroutine method here to execute coroutines.
 */
public class MainThreadDispatcher : MonoBehaviour
{
	/**
	 * enqueue a coroutine to this queue so that it will be executed.
	 * this is neccessary so a worker thread can tell the main thread to execute a task.
	 */
	private ConcurrentQueue<IEnumerator> coroutineQueue = new ConcurrentQueue<IEnumerator>();
	private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

	void Start()
	{
		StartCoroutine(processQueue());
	}

	private IEnumerator processQueue()
	{
		while (true)
		{
			while (coroutineQueue.TryDequeue(out IEnumerator task))
			{
				startCoroutine(task);
			}
			while (actionQueue.TryDequeue(out Action task))
			{
				task?.Invoke();
			}
			yield return new WaitForSeconds(0.5f);
		}

	}

	public void enqueue(Action task)
	{
		actionQueue.Enqueue(task);
	}

	public void enqueue(IEnumerator coroutine)
	{
		coroutineQueue.Enqueue(coroutine);
	}


	public Coroutine startCoroutine(IEnumerator coroutine)
	{
		return StartCoroutine(coroutine);
	}

	public void stopCoroutine(Coroutine coroutine)
	{
		StopCoroutine(coroutine);
	}

}
