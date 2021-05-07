using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.AI;
using UnityEngine;

namespace Level
{
	public class SpawnScarecrowOnLootActivate : MonoBehaviour
	{
		[SerializeField]
		private MapReward _mapReward;

		[SerializeField]
		private GameObject _scareCrawPrefab;

		private List<ScareCrawAI> _scarCraws;

		private void Start()
		{
			_scarCraws = new List<ScareCrawAI>(base.transform.childCount);
			ScareCrawAI[] componentsInChildren = GetComponentsInChildren<ScareCrawAI>();
			foreach (ScareCrawAI scareCraw in componentsInChildren)
			{
				scareCraw.character.health.onDied += delegate
				{
					StartCoroutine(Revive(scareCraw));
				};
				scareCraw.character.gameObject.SetActive(false);
				_scarCraws.Add(scareCraw);
			}
			_mapReward.onLoot += delegate
			{
				foreach (ScareCrawAI scarCraw in _scarCraws)
				{
					scarCraw.character.gameObject.SetActive(true);
					scarCraw.Appear();
				}
			};
		}

		private IEnumerator Revive(ScareCrawAI scareCraw)
		{
			yield return Chronometer.global.WaitForSeconds(3f);
			GameObject gameObject = Object.Instantiate(_scareCrawPrefab, scareCraw.character.transform.position, Quaternion.identity, base.transform);
			Character component = gameObject.GetComponent<Character>();
			ScareCrawAI scareCrawAI = gameObject.GetComponentInChildren<ScareCrawAI>();
			component.ForceToLookAt(scareCraw.character.lookingDirection);
			component.gameObject.SetActive(true);
			component.health.onDied += delegate
			{
				StartCoroutine(Revive(scareCrawAI));
			};
			scareCrawAI.Appear();
			Object.Destroy(scareCraw.gameObject);
		}
	}
}
