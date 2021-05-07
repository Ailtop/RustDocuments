using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using TMPro;
using UnityEngine;

namespace Characters
{
	public class Dummy : MonoBehaviour
	{
		private static Stat.Values cannotBeKnockbacked = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.KnockbackResistance, 0.0));

		private static readonly string[] tauntScripts = new string[8] { "You want to touch me?", "Bring it!", "Come on!", "Your halo is mine!", "Let's dance!", "You've been naughty!", "You bastard!", "Your weakness!" };

		private readonly GetInvulnerable _getInvulnerable = new GetInvulnerable
		{
			duration = 3f
		};

		[SerializeField]
		private bool _immuneToCritical;

		[SerializeField]
		private bool _cannotBeKnockbacked;

		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private TMP_Text _timeText;

		[SerializeField]
		private TMP_Text _dpsText;

		private bool _started;

		private float _time;

		private double _damage;

		private Vector3 _original;

		private void Awake()
		{
			_character.health.onTookDamage += OnTookDamage;
			if (_immuneToCritical)
			{
				_character.health.immuneToCritical = true;
			}
			if (_cannotBeKnockbacked)
			{
				_character.stat.AttachValues(cannotBeKnockbacked);
			}
			_original = base.transform.position;
			_dpsText.text = tauntScripts.Random();
		}

		private void Initialize()
		{
			_timeText.text = $"{_time:0.00}s";
			_dpsText.text = $"{_damage:0.0}\n{_damage / (double)_time:0.00}";
			_started = false;
			_time = 0f;
			_damage = 0.0;
			base.transform.position = _original;
			_character.health.Revive();
			_character.movement.push.Expire();
			_character.health.ResetToMaximumHealth();
			_character.ability.Add(_getInvulnerable);
			StopAllCoroutines();
		}

		private void Update()
		{
			if (_started)
			{
				_timeText.color = Color.yellow;
				_dpsText.color = Color.yellow;
			}
			else if (_character.invulnerable.value)
			{
				_timeText.color = Color.red;
				_dpsText.color = Color.red;
			}
			else
			{
				_timeText.color = Color.white;
				_dpsText.color = Color.white;
			}
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!_character.invulnerable.value)
			{
				if (!_started)
				{
					_started = true;
					StartCoroutine(CMesure());
				}
				double damage = _damage;
				Damage damage2 = tookDamage;
				_damage = damage + damage2.amount;
				if (_character.health.currentHealth == 0.0)
				{
					Initialize();
				}
			}
		}

		private IEnumerator CMesure()
		{
			while (true)
			{
				yield return null;
				_time += Chronometer.global.deltaTime;
				_timeText.text = $"{_time:0.00}s";
				_dpsText.text = $"{_damage:0.0}\n{_damage / (double)_time:0.00}";
			}
		}
	}
}
