using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Pope
{
	public abstract class LadderSelectPolicy : MonoBehaviour
	{
		[SerializeField]
		private Transform _spawnPointContainer;

		private FanaticLadder[] _fanaticLadders;

		private void Awake()
		{
			_fanaticLadders = new FanaticLadder[_spawnPointContainer.childCount];
			int num = 0;
			foreach (Transform item in _spawnPointContainer)
			{
				_fanaticLadders[num++] = item.GetComponent<FanaticLadder>();
			}
		}

		public IEnumerator<FanaticLadder> GetLadders()
		{
			return SelectLadders().GetEnumerator() as IEnumerator<FanaticLadder>;
		}

		public abstract FanaticLadder[] SelectLadders();
	}
}
