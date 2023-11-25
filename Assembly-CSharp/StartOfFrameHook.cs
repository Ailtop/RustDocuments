using System;
using UnityEngine;

[DisallowMultipleComponent]
public class StartOfFrameHook : MonoBehaviour
{
	public static Action OnStartOfFrame;

	private void OnEnable()
	{
		OnStartOfFrame?.Invoke();
	}

	private void Update()
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
	}
}
