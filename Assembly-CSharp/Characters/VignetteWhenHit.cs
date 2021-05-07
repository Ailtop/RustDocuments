using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters
{
	public class VignetteWhenHit : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _owner;

		[SerializeField]
		[Subcomponent(typeof(Vignette))]
		private Vignette _vignette;

		private void Awake()
		{
			_owner.health.onTookDamage += onTookDamage;
		}

		private void onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (damageDealt > 0.0 && tookDamage.attackType != Damage.AttackType.Additional)
			{
				_vignette.Run(_owner);
			}
		}
	}
}
