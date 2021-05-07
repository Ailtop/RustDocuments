using System;
using Characters.Gear.Weapons.Gauges;
using FX;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class MummyPassive : Ability, IAbilityInstance
	{
		[Serializable]
		private class GunMap
		{
			public string key;

			public int ammo;

			public Color gaugeColor;

			public EffectInfo losingEffect;

			public PolymorphBody polymorphBody;
		}

		[SerializeField]
		private ValueGauge _gauge;

		[Space]
		[SerializeField]
		private GunMap[] _gunMaps;

		private GunMap _current;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached { get; private set; }

		public Sprite icon => null;

		public int iconStacks => 0;

		public float iconFillAmount => 0f;

		public bool expired => false;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		private GunMap GetGunMap(string key)
		{
			GunMap[] gunMaps = _gunMaps;
			foreach (GunMap gunMap in gunMaps)
			{
				if (gunMap.key.Equals(key, StringComparison.OrdinalIgnoreCase))
				{
					return gunMap;
				}
			}
			return null;
		}

		public void PickUpWeapon(string key)
		{
			GunMap gunMap = GetGunMap(key);
			if (gunMap == null)
			{
				Debug.LogError("There is no Mummy gun for key " + key);
				return;
			}
			_gauge.maxValue = gunMap.ammo;
			_gauge.Set(gunMap.ammo);
			_gauge.defaultBarColor = gunMap.gaugeColor;
			if (_current == gunMap)
			{
				return;
			}
			if (!attached)
			{
				_current = gunMap;
				_current.polymorphBody.character = owner;
				return;
			}
			if (_current != null)
			{
				_current.losingEffect.Spawn(owner.transform.position, owner);
				_current.polymorphBody.EndPolymorph();
			}
			_current = gunMap;
			_current.polymorphBody.character = owner;
			_current.polymorphBody.StartPolymorph();
		}

		public void UpdateTime(float deltaTime)
		{
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			attached = true;
			_gauge.onChanged += OnGaugeChanged;
			if (_current != null)
			{
				_current.polymorphBody.StartPolymorph();
			}
		}

		public void Detach()
		{
			attached = false;
			_gauge.onChanged -= OnGaugeChanged;
			if (_current != null)
			{
				_current.polymorphBody.EndPolymorph();
			}
		}

		private void OnGaugeChanged(float oldValue, float newValue)
		{
			if (!(newValue > 0f) && oldValue != newValue && _current != null)
			{
				_current.losingEffect.Spawn(owner.transform.position, owner);
				_current.polymorphBody.EndPolymorph();
				_current = null;
			}
		}
	}
}
