using System;
using Services;
using Singletons;
using UnityEngine;

[Serializable]
public class GoldPossibility
{
	[SerializeField]
	[Range(0f, 100f)]
	private float _possibility;

	[SerializeField]
	private int _min;

	[SerializeField]
	private int _max;

	[SerializeField]
	private int _goldAmountPerCoin;

	public bool Drop(Vector3 position)
	{
		if (!MMMaths.Chance(_possibility / 100f))
		{
			return false;
		}
		int num = UnityEngine.Random.Range(_min, _max);
		Singleton<Service>.Instance.levelManager.DropGold(num, Mathf.Max(num / _goldAmountPerCoin, 1), position);
		return true;
	}
}
