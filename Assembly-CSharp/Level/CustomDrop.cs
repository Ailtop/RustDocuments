using System.Collections;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class CustomDrop : MonoBehaviour
	{
		[SerializeField]
		private PoolObject _poolObject;

		[SerializeField]
		private float _lifetime = 30f;

		private LevelManager _levelManager;

		private void Awake()
		{
			_levelManager = Singleton<Service>.Instance.levelManager;
		}

		private void OnEnable()
		{
			_levelManager.RegisterDrop(_poolObject);
			StartCoroutine(CDespawnAfterLifetime());
		}

		private void OnDisable()
		{
			if (!Service.quitting)
			{
				_levelManager.DeregisterDrop(_poolObject);
			}
		}

		private IEnumerator CDespawnAfterLifetime()
		{
			yield return Chronometer.global.WaitForSeconds(_lifetime);
			_poolObject.Despawn();
		}
	}
}
