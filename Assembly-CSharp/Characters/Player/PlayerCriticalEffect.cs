using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerCriticalEffect : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(Vignette))]
		private Vignette _vignette;

		private void Awake()
		{
			Character character = _character;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!(target.character == null))
			{
				Damage damage = tookDamage;
				if (!(damage.amount <= 0.0) && tookDamage.critical)
				{
					Chronometer.global.AttachTimeScale(this, 0.2f, 0.1f);
					_character.chronometer.master.AttachTimeScale(this, 5f, 0.1f);
					_vignette.Run(_character);
				}
			}
		}
	}
}
