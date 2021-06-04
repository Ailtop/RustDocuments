using System;
using UnityEngine;

public class AIThinkManager : BaseMonoBehaviour, IServerComponent
{
	public enum QueueType
	{
		Human,
		Animal
	}

	public static ListHashSet<IThinker> _processQueue = new ListHashSet<IThinker>();

	public static ListHashSet<IThinker> _removalQueue = new ListHashSet<IThinker>();

	public static ListHashSet<IThinker> _animalProcessQueue = new ListHashSet<IThinker>();

	public static ListHashSet<IThinker> _animalremovalQueue = new ListHashSet<IThinker>();

	[ServerVar]
	[Help("How many miliseconds to budget for processing AI entities per server frame")]
	public static float framebudgetms = 2.5f;

	[Help("How many miliseconds to budget for processing animal AI entities per server frame")]
	[ServerVar]
	public static float animalframebudgetms = 2.5f;

	private static int lastIndex = 0;

	private static int lastAnimalIndex = 0;

	public static void ProcessQueue(QueueType queueType)
	{
		if (queueType == QueueType.Human)
		{
			DoRemoval(_removalQueue, _processQueue);
			AIInformationZone.BudgetedTick();
		}
		else
		{
			DoRemoval(_animalremovalQueue, _animalProcessQueue);
		}
		if (queueType == QueueType.Human)
		{
			DoProcessing(_processQueue, framebudgetms / 1000f, ref lastIndex);
		}
		else
		{
			DoProcessing(_animalProcessQueue, animalframebudgetms / 1000f, ref lastAnimalIndex);
		}
	}

	private static void DoRemoval(ListHashSet<IThinker> removal, ListHashSet<IThinker> process)
	{
		if (removal.Count <= 0)
		{
			return;
		}
		foreach (IThinker item in removal)
		{
			process.Remove(item);
		}
		removal.Clear();
	}

	private static void DoProcessing(ListHashSet<IThinker> process, float budgetSeconds, ref int last)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (last < process.Count && Time.realtimeSinceStartup < realtimeSinceStartup + budgetSeconds)
		{
			IThinker thinker = process[last];
			if (thinker != null)
			{
				try
				{
					thinker.TryThink();
				}
				catch (Exception message)
				{
					Debug.LogWarning(message);
				}
			}
			last++;
		}
		if (last >= process.Count)
		{
			last = 0;
		}
	}

	public static void Add(IThinker toAdd)
	{
		_processQueue.Add(toAdd);
	}

	public static void Remove(IThinker toRemove)
	{
		_removalQueue.Add(toRemove);
	}

	public static void AddAnimal(IThinker toAdd)
	{
		_animalProcessQueue.Add(toAdd);
	}

	public static void RemoveAnimal(IThinker toRemove)
	{
		_animalremovalQueue.Add(toRemove);
	}
}
