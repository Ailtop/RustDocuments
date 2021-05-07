using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class PillarOfLightContainerPool : MonoBehaviour
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private float _delay;

		private List<PillarOfLightContainer> _pool;

		private void Start()
		{
			_pool = new List<PillarOfLightContainer>(_container.childCount);
			foreach (Transform item in _container)
			{
				_pool.Add(item.GetComponent<PillarOfLightContainer>());
			}
		}

		public void Run(Character owner)
		{
			StartCoroutine(CRun(owner));
		}

		private IEnumerator CRun(Character owner)
		{
			PillarOfLightContainer selected = _pool.Random();
			selected.gameObject.SetActive(true);
			selected.Sign(owner);
			yield return owner.chronometer.master.WaitForSeconds(_delay);
			selected.Attack(owner);
		}
	}
}
