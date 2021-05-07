using System.Collections;
using Characters.Actions;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Summon
{
	public class SummonCharacter : CharacterOperation
	{
		[SerializeField]
		private Character _characterToSummon;

		[SerializeField]
		private Action _action;

		private Character _owner;

		private void Awake()
		{
			_characterToSummon.gameObject.SetActive(false);
			_characterToSummon.transform.parent = null;
		}

		public override void Run(Character owner)
		{
			_owner = owner;
			if (!_characterToSummon.gameObject.activeSelf)
			{
				StartCoroutine(CUse());
			}
		}

		public override void Stop()
		{
			_characterToSummon.gameObject.SetActive(false);
		}

		private void OnEnable()
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += Stop;
		}

		private void OnDisable()
		{
			Stop();
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= Stop;
		}

		private IEnumerator CUse()
		{
			_characterToSummon.lookingDirection = _owner.desiringLookingDirection;
			_characterToSummon.transform.position = base.transform.position;
			_characterToSummon.gameObject.SetActive(true);
			_action?.TryStart();
			while (_action.running)
			{
				yield return null;
			}
			_characterToSummon.gameObject.SetActive(false);
		}

		protected override void OnDestroy()
		{
			if (!Service.quitting)
			{
				Object.Destroy(_characterToSummon.gameObject);
			}
		}
	}
}
