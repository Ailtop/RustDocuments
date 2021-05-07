using System;
using System.Collections;
using UnityEngine;

public class Floating : MonoBehaviour
{
	[SerializeField]
	private float _heightRange;

	[SerializeField]
	private float _speed = 1f;

	private float _current;

	private Vector2 _start;

	private void Awake()
	{
		StartCoroutine(CRun());
	}

	private IEnumerator CRun()
	{
		_current = UnityEngine.Random.Range(0f, 1f);
		_start = base.transform.position;
		while (true)
		{
			_current += Chronometer.global.deltaTime * _speed;
			float num = Mathf.Sin(_current * 360f * ((float)Math.PI / 180f)) * _heightRange;
			Vector2 start = _start;
			start.y += num;
			base.transform.position = start;
			yield return null;
		}
	}
}
