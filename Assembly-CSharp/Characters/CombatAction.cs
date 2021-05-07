using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters
{
	public class CombatAction : MonoBehaviour
	{
		private class MightyBlow
		{
			private const float _timeout = 0.5f;

			private Stat.Value _movementSpeed;

			private Stat.Values _stat;

			private readonly Character _owner;

			private int _streaks;

			private float _remainTime = 0.5f;

			public int streaks
			{
				get
				{
					return _streaks;
				}
				set
				{
					if (_streaks != value)
					{
						_streaks = value;
						_remainTime = 0.5f;
						UpdateStat();
					}
				}
			}

			internal MightyBlow(Character owner)
			{
				_owner = owner;
				_movementSpeed = new Stat.Value(Stat.Category.Percent, Stat.Kind.MovementSpeed, 1.0);
				_stat = new Stat.Values(_movementSpeed);
			}

			internal void Update(float deltaTime)
			{
				_remainTime -= deltaTime;
				if (_remainTime <= 0f)
				{
					_remainTime = 0.5f;
					streaks = 0;
				}
			}

			internal void UpdateStat()
			{
				float duration = 0f;
				if (_streaks > 5)
				{
					_movementSpeed.value = 1.1000000238418579;
					duration = 3f;
				}
				else if (_streaks > 7)
				{
					_movementSpeed.value = 1.2000000476837158;
					duration = 4f;
				}
				else if (_streaks > 10)
				{
					_movementSpeed.value = 1.2999999523162842;
					duration = 5f;
				}
				_owner.stat.AttachOrUpdateTimedValues(_stat, duration);
			}
		}

		private class Massacre
		{
			private const float _defaultTimeout = 10f;

			private Stat.Value _movementSpeed;

			private Stat.Values _stat;

			private readonly Character _owner;

			private int _streaks;

			private float _remainTime = 10f;

			public int streaks
			{
				get
				{
					return _streaks;
				}
				set
				{
					if (_streaks != value)
					{
						_streaks = value;
						_remainTime = 5f;
						if (streaks > 100)
						{
							_remainTime = 1f;
						}
						else if (streaks > 75)
						{
							_remainTime = 2f;
						}
						else if (streaks > 50)
						{
							_remainTime = 3f;
						}
						else if (streaks > 25)
						{
							_remainTime = 4f;
						}
						else if (streaks > 10)
						{
							_remainTime = 5f;
						}
						UpdateStat();
					}
				}
			}

			internal Massacre(Character owner)
			{
				_owner = owner;
				_movementSpeed = new Stat.Value(Stat.Category.Percent, Stat.Kind.MovementSpeed, 1.0);
				_stat = new Stat.Values(_movementSpeed);
			}

			internal void Update(float deltaTime)
			{
				_remainTime -= deltaTime;
				if (_remainTime <= 0f)
				{
					_remainTime = 10f;
					streaks = 0;
				}
			}

			internal void UpdateStat()
			{
				float duration = 0f;
				if (streaks > 100)
				{
					_movementSpeed.value = 1.5;
					duration = 10f;
				}
				else if (streaks > 75)
				{
					_movementSpeed.value = 1.3999999761581421;
					duration = 10f;
				}
				else if (streaks > 50)
				{
					_movementSpeed.value = 1.2999999523162842;
					duration = 10f;
				}
				else if (streaks > 25)
				{
					_movementSpeed.value = 1.2000000476837158;
					duration = 10f;
				}
				else if (streaks > 10)
				{
					_movementSpeed.value = 1.1000000238418579;
					duration = 10f;
				}
				_owner.stat.AttachOrUpdateTimedValues(_stat, duration);
			}
		}

		[SerializeField]
		[GetComponent]
		private Character _character;

		private MightyBlow _mightyBlow;

		private Massacre _massacre;

		private void Awake()
		{
			_mightyBlow = new MightyBlow(_character);
			_massacre = new Massacre(_character);
			Character character = _character;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(onGaveDamage));
		}

		private void Update()
		{
			_mightyBlow.Update(_character.chronometer.master.deltaTime);
			_massacre.Update(_character.chronometer.master.deltaTime);
		}

		private void onGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!(target.character == null) && !(target.character.health.currentHealth > 0.0))
			{
				_mightyBlow.streaks++;
				_massacre.streaks++;
			}
		}
	}
}
