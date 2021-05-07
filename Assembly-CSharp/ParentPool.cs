using System;
using System.Collections.Generic;
using UnityEngine;

public class ParentPool : MonoBehaviour
{
	[Serializable]
	private class EffectrRangeKeyValue
	{
		[SerializeField]
		internal Transform effect;

		[SerializeField]
		internal Transform range;
	}

	[SerializeField]
	private List<EffectrRangeKeyValue> _parents;

	private Transform _currentEffectParent;

	private Transform _currentAttackParent;

	public Transform currentEffectParent
	{
		get
		{
			return _currentEffectParent;
		}
		private set
		{
			_currentEffectParent = value;
		}
	}

	private void Awake()
	{
		foreach (EffectrRangeKeyValue parent in _parents)
		{
			parent.range.gameObject.SetActive(false);
		}
	}

	public Transform GetRandomParent()
	{
		EffectrRangeKeyValue effectrRangeKeyValue = _parents.Random();
		currentEffectParent = effectrRangeKeyValue.effect;
		_currentAttackParent = effectrRangeKeyValue.range;
		PickOneAttackRange();
		return currentEffectParent;
	}

	public Transform GetFirstParent()
	{
		EffectrRangeKeyValue effectrRangeKeyValue = _parents[0];
		currentEffectParent = effectrRangeKeyValue.effect;
		_currentAttackParent = effectrRangeKeyValue.range;
		PickOneAttackRange();
		return currentEffectParent;
	}

	private void PickOneAttackRange()
	{
		foreach (EffectrRangeKeyValue parent in _parents)
		{
			Transform range = parent.range;
			if (range != _currentAttackParent)
			{
				range.gameObject.SetActive(false);
			}
			else
			{
				range.gameObject.SetActive(true);
			}
		}
	}
}
