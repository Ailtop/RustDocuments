using System;
using Services;
using Singletons;
using UnityEngine;

namespace Characters
{
	public class Health : MonoBehaviour
	{
		public delegate void HealedDelegate(double healed, double overHealed);

		private readonly TakeDamageEvent _onTakeDamage = new TakeDamageEvent();

		public bool immuneToCritical;

		private Collider2D _collider;

		public PriorityList<TakeDamageDelegate> onTakeDamage => _onTakeDamage;

		public Character owner { get; set; }

		public Shield shield { get; private set; } = new Shield();


		public double currentHealth { get; private set; }

		public double maximumHealth { get; private set; }

		public double percent { get; private set; }

		public bool dead { get; private set; }

		public event TookDamageDelegate onTookDamage;

		public event Action onDie;

		public event Action onDied;

		public event Action onDiedTryCatch;

		public event Action onChanged;

		public event HealedDelegate onHealed;

		private void Awake()
		{
			_collider = GetComponentInChildren<Collider2D>();
		}

		public void SetHealth(double current, double maximum)
		{
			currentHealth = ((current < maximum) ? current : maximum);
			maximumHealth = maximum;
			UpdateHealth();
		}

		public void SetCurrentHealth(double health)
		{
			currentHealth = health;
			UpdateHealth();
		}

		public void SetMaximumHealth(double health)
		{
			maximumHealth = health;
			if (currentHealth > maximumHealth)
			{
				currentHealth = health;
			}
			UpdateHealth();
		}

		public bool TakeDamage(ref Damage damage)
		{
			double dealtDamage;
			return TakeDamage(ref damage, out dealtDamage);
		}

		public bool TakeDamage(ref Damage damage, out double dealtDamage)
		{
			damage.Evaluate(immuneToCritical);
			Damage originalDamage = damage;
			if (_onTakeDamage.Invoke(ref damage))
			{
				damage.@base = 0.0;
				dealtDamage = 0.0;
				return true;
			}
			double amount = damage.amount;
			double num = shield.Consume(amount);
			double num2 = amount - num;
			double num3 = TakeHealth(num);
			this.onTookDamage?.Invoke(ref originalDamage, ref damage, num3 + num2);
			dealtDamage = num3 + num2;
			return false;
		}

		public double TakeHealth(double amount)
		{
			if (dead)
			{
				return 0.0;
			}
			currentHealth -= amount;
			if (currentHealth <= 0.0)
			{
				double num = currentHealth;
				currentHealth = 0.0;
				try
				{
					this.onDie?.Invoke();
					if (currentHealth <= 0.0)
					{
						dead = true;
						this.onDied?.Invoke();
						this.onDiedTryCatch?.Invoke();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Eror while running onDie or onDied of " + base.name + " : " + ex.Message);
					currentHealth = 0.0;
					dead = true;
					this.onDiedTryCatch?.Invoke();
				}
				UpdateHealth();
				return amount + num;
			}
			UpdateHealth();
			return amount;
		}

		public double PercentHeal(float percent)
		{
			return Heal(maximumHealth * (double)percent);
		}

		public double Heal(double amount, bool notify = true)
		{
			return Heal(ref amount, notify);
		}

		public double Heal(ref double amount, bool notify = true)
		{
			amount *= owner.stat.GetFinal(Stat.Kind.TakingHealAmount);
			double num = 0.0;
			currentHealth += amount;
			if (currentHealth > maximumHealth)
			{
				num = currentHealth - maximumHealth;
				amount -= num;
				currentHealth = maximumHealth;
			}
			UpdateHealth();
			if (notify)
			{
				if (_collider == null)
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnHeal(amount, base.transform.position);
				}
				else
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnHeal(amount, MMMaths.RandomPointWithinBounds(_collider.bounds));
				}
				this.onHealed?.Invoke(amount, num);
			}
			return num;
		}

		public void ResetToMaximumHealth()
		{
			SetCurrentHealth(maximumHealth);
		}

		public void Revive()
		{
			Revive(maximumHealth);
		}

		public void Revive(double health)
		{
			dead = false;
			SetCurrentHealth(health);
		}

		public void Kill()
		{
			if (!dead)
			{
				currentHealth = 0.0;
				dead = true;
				UpdateHealth();
				try
				{
					this.onDie?.Invoke();
					this.onDied?.Invoke();
					this.onDiedTryCatch?.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogError("Eror while running onDie or onDied of " + base.name + " : " + ex.Message);
					this.onDiedTryCatch?.Invoke();
				}
			}
		}

		private void UpdateHealth()
		{
			percent = currentHealth / maximumHealth;
			this.onChanged?.Invoke();
		}
	}
}
