using System.Collections;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Movements
{
	[RequireComponent(typeof(Movement))]
	public class StuckResolver : MonoBehaviour
	{
		private const float _checkInterval = 3f;

		private static readonly WaitForSeconds _waitForCheck = new WaitForSeconds(3f);

		private const float _stuckCheckInterval = 0.1f;

		private static readonly WaitForSeconds _waitForStuckCheck = new WaitForSeconds(0.1f);

		private const int _stuckCheckCount = 10;

		private CharacterController2D _controller;

		private Vector3 _lastValidPosition;

		private void Awake()
		{
			_controller = GetComponent<CharacterController2D>();
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += StartCheck;
			Singleton<Service>.Instance.levelManager.onMapLoaded += StopCheck;
		}

		private void OnDestroy()
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= StartCheck;
			Singleton<Service>.Instance.levelManager.onMapLoaded -= StopCheck;
		}

		private void StartCheck()
		{
			_lastValidPosition = Map.Instance.playerOrigin;
			StartCoroutine(CCheck());
		}

		private void StopCheck()
		{
			StopAllCoroutines();
		}

		private IEnumerator CCheck()
		{
			while (true)
			{
				yield return _waitForCheck;
				if (!_controller.IsInTerrain())
				{
					_lastValidPosition = base.transform.position;
					continue;
				}
				bool stuck = true;
				for (int i = 0; i < 10; i++)
				{
					yield return _waitForStuckCheck;
					if (!_controller.IsInTerrain())
					{
						stuck = false;
						break;
					}
				}
				if (stuck)
				{
					Debug.Log("The character " + base.name + " is stucked in the terrain. It was moved to last valid position.");
					base.transform.position = _lastValidPosition;
					_controller.ResetBounds();
					Physics2D.SyncTransforms();
				}
			}
		}
	}
}
