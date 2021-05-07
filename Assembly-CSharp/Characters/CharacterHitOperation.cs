using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters
{
	public class CharacterHitOperation : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[GetComponent]
		private CharacterHealth _health;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _hitOperations;

		private void Awake()
		{
			_health.onTookDamage += OnTookDamage;
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!_health.dead)
			{
				StartCoroutine(_hitOperations.CRun(_character));
			}
		}
	}
}
