using System;
using UnityEngine;

public class Floater : MonoBehaviour
{
	[SerializeField]
	private float _amplitude = 0.2f;

	[SerializeField]
	private float _frequency = 1f;

	private Vector3 _originalPosition;

	private Vector3 _floatingPosition;

	private void Start()
	{
		_originalPosition = base.transform.position;
	}

	private void Update()
	{
		_floatingPosition = _originalPosition;
		_floatingPosition.y += Mathf.Sin(Time.fixedTime * (float)Math.PI * _frequency) * _amplitude;
		base.transform.position = _floatingPosition;
	}
}
