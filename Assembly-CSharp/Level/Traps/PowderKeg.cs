using Characters;
using Characters.Abilities;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	public class PowderKeg : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private GameObject _remain1;

		[SerializeField]
		private GameObject _remain2;

		[SerializeField]
		private ParticleEffectInfo _particleEffectInfo;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operationsOnDie;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher _abilityAttacher;

		private void Awake()
		{
			_operationsOnDie.Initialize();
			_abilityAttacher.Initialize(_character);
			_abilityAttacher.StartAttach();
			_character.health.onDie += Run;
		}

		private void OnDestroy()
		{
			_abilityAttacher.StopAttach();
			_character.health.onDie -= Run;
		}

		private void Run()
		{
			_character.health.onDie -= Run;
			_particleEffectInfo.Emit(_character.transform.position, _character.collider.bounds, Vector2.up * 3f);
			if (MMMaths.RandomBool())
			{
				_remain1.gameObject.SetActive(true);
			}
			else
			{
				_remain2.gameObject.SetActive(true);
			}
			_character.@base.gameObject.SetActive(false);
			StartCoroutine(_operationsOnDie.CRun(_character));
		}
	}
}
