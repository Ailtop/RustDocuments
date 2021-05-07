using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class PillarOfLight : MonoBehaviour
	{
		[SerializeField]
		private GameObject _sign;

		[SerializeField]
		private GameObject _attack;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _attackOperations;

		private bool _registered;

		private void Awake()
		{
			_attackOperations.Initialize();
		}

		public void Sign(Character owner)
		{
			_sign.SetActive(true);
			if (!_registered)
			{
				owner.health.onDied += delegate
				{
					_sign.SetActive(false);
				};
			}
			_registered = true;
		}

		public void Attack(Character owner)
		{
			_sign.SetActive(false);
			_attackOperations.gameObject.SetActive(true);
			_attackOperations.Run(owner);
		}
	}
}
