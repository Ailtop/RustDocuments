using System.Collections.Generic;
using UnityEngine;

public class SufflePositions : MonoBehaviour
{
	[SerializeField]
	private bool _suffleOnAwake = true;

	private Transform[] _childs;

	private List<Vector3> _positions = new List<Vector3>();

	private void Awake()
	{
		Initialize();
		if (_suffleOnAwake)
		{
			Shuffle();
		}
	}

	public void Shuffle()
	{
		_positions.Shuffle();
		for (int i = 0; i < _childs.Length; i++)
		{
			_childs[i].transform.position = _positions[i];
		}
	}

	private void Initialize()
	{
		_childs = new Transform[base.transform.childCount];
		_positions.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			_childs[i] = base.transform.GetChild(i);
			_positions.Add(_childs[i].position);
		}
	}
}
