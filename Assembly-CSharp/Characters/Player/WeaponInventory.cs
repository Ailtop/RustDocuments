using System;
using System.Collections;
using System.Linq;
using Characters.Controllers;
using Characters.Gear;
using Characters.Gear.Weapons;
using FX;
using FX.SpriteEffects;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Player
{
	public sealed class WeaponInventory : MonoBehaviour, IAttackDamage
	{
		public delegate void OnChangeDelegate(Weapon old, Weapon @new);

		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[GetComponent]
		private PlayerInput _input;

		[SerializeField]
		private Weapon _default;

		private const float _swapCooldown = 8f;

		private float _remainCooldown;

		public readonly Weapon[] weapons = new Weapon[2];

		private EffectInfo _switchEffect;

		public int currentIndex { get; private set; } = -1;


		public Weapon current { get; private set; }

		public Weapon polymorphWeapon { get; private set; }

		public Weapon polymorphOrCurrent
		{
			get
			{
				if (!(polymorphWeapon == null))
				{
					return polymorphWeapon;
				}
				return current;
			}
		}

		public Weapon next
		{
			get
			{
				int num = currentIndex;
				do
				{
					num++;
					if (num == weapons.Length)
					{
						num = 0;
					}
					if (num == currentIndex)
					{
						return null;
					}
				}
				while (weapons[num] == null);
				return weapons[num];
			}
		}

		public IAttackDamage weaponAttackDamage { get; private set; }

		public float amount => weaponAttackDamage.amount;

		public float reaminCooldownPercent => _remainCooldown / 8f;

		public bool swapReady { get; private set; } = true;


		public event Action onSwap;

		public event OnChangeDelegate onChanged;

		public event Action onSwapReady;

		private void Awake()
		{
			_default = _default.Instantiate();
			ForceEquip(_default);
			_switchEffect = new EffectInfo(Resource.instance.swapEffect)
			{
				chronometer = _character.chronometer.effect
			};
		}

		private void OnDisable()
		{
			PlayerInput.blocked.Detach(this);
		}

		private void Update()
		{
			if (_remainCooldown > 0f)
			{
				swapReady = false;
				_remainCooldown -= _character.chronometer.master.deltaTime * _character.stat.GetSwapCooldownSpeed();
				return;
			}
			if (!swapReady)
			{
				this.onSwapReady?.Invoke();
				swapReady = true;
			}
			_remainCooldown = 0f;
		}

		private void StartUse(Weapon weapon)
		{
			weapon.gameObject.SetActive(true);
			_character.CancelAction();
			_character.InitializeActions();
			_character.animationController.Initialize();
			_character.animationController.ForceUpdate();
			_character.collider.size = weapon.hitbox.size;
			_character.collider.offset = weapon.hitbox.offset;
			weaponAttackDamage = weapon.GetComponent<IAttackDamage>();
			weapon.StartUse();
		}

		private void EndUse(Weapon weapon)
		{
			weapon.EndUse();
			_character.stat.DetachValues(weapon.stat);
		}

		public bool NextWeapon(bool force = false)
		{
			if (!force && (_remainCooldown > 0f || _character.stunedOrFreezed))
			{
				return false;
			}
			for (int i = 1; i < weapons.Length; i++)
			{
				int num = (currentIndex + i) % weapons.Length;
				if (weapons[num] != null)
				{
					ChangeWeaponWithSwitchAction(num);
					_remainCooldown = 8f;
					return true;
				}
			}
			return false;
		}

		private void ChangeWeapon(int index)
		{
			Unpolymorph();
			polymorphWeapon = null;
			current.gameObject.SetActive(false);
			current.EndUse();
			currentIndex = index;
			current = weapons[currentIndex];
			StartUse(current);
			this.onSwap?.Invoke();
		}

		private void ChangeWeaponWithSwitchAction(int index)
		{
			Effects.SpritePoolObject spritePoolObject = Effects.sprite.Spawn();
			SpriteRenderer spriteRenderer = spritePoolObject.spriteRenderer;
			SpriteRenderer spriteRenderer2 = current.characterAnimation.spriteRenderer;
			spriteRenderer.sprite = spriteRenderer2.sprite;
			spriteRenderer.transform.localScale = spriteRenderer2.transform.lossyScale;
			spriteRenderer.transform.SetPositionAndRotation(spriteRenderer2.transform.position, spriteRenderer2.transform.rotation);
			spriteRenderer.flipX = spriteRenderer2.flipX;
			spriteRenderer.flipY = spriteRenderer2.flipY;
			spriteRenderer.sortingLayerID = spriteRenderer2.sortingLayerID;
			spriteRenderer.sortingOrder = spriteRenderer2.sortingOrder - 1;
			spriteRenderer.color = new Color(71f / 85f, 46f / 255f, 1f);
			spriteRenderer.sharedMaterial = Materials.color;
			spritePoolObject.FadeOut(_character.chronometer.effect, AnimationCurve.Linear(0f, 0f, 1f, 1f), 0.5f);
			ChangeWeapon(index);
			current.StartSwitchAction();
			_switchEffect.Spawn(base.transform.position);
		}

		public void LoseAll()
		{
			Unpolymorph();
			current.EndUse();
			ChangeWeapon(0);
			for (int i = 1; i < weapons.Length; i++)
			{
				Weapon weapon = weapons[i];
				if (!(weapon == null))
				{
					Drop(weapon);
					this.onChanged?.Invoke(weapon, null);
					UnityEngine.Object.Destroy(weapon.gameObject);
				}
			}
			_character.InitializeActions();
			_character.animationController.Initialize();
			_character.animationController.ForceUpdate();
		}

		public void Unequip(Weapon weapon)
		{
			int num = -1;
			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapon == weapons[i])
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				Drop(weapon);
				_character.InitializeActions();
				_character.animationController.Initialize();
				_character.animationController.ForceUpdate();
				weapons[num] = null;
				this.onChanged?.Invoke(weapon, null);
			}
		}

		private void Drop(Weapon weapon)
		{
			EndUse(weapon);
			weapon.state = Characters.Gear.Gear.State.Dropped;
		}

		public Weapon Equip(Weapon weapon)
		{
			int index = currentIndex;
			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapons[i] == null)
				{
					index = i;
					break;
				}
			}
			_character.spriteEffectStack.Add(new EasedColorOverlay(int.MaxValue, Color.white, new Color(1f, 1f, 1f, 0f), new Curve(AnimationCurve.Linear(0f, 0f, 1f, 1f), 1f, 0.15f)));
			return EquipAt(weapon, index);
		}

		public Weapon EquipAt(Weapon weapon, int index)
		{
			Unpolymorph();
			for (int i = 0; i < weapons.Length; i++)
			{
				if (index != i && weapons[i] != null)
				{
					weapons[i].gameObject.SetActive(false);
					weapons[i].EndUse();
				}
			}
			Weapon weapon2 = weapons[index];
			if (weapon2 != null)
			{
				Drop(weapon2);
			}
			current = weapon;
			current.transform.parent = _character.@base;
			current.transform.localPosition = Vector3.zero;
			current.transform.localScale = Vector3.one;
			StartUse(current);
			weapons[index] = weapon;
			currentIndex = index;
			this.onChanged?.Invoke(weapon2, weapon);
			return weapon2;
		}

		public void ForceEquip(Weapon weapon)
		{
			weapon.state = Characters.Gear.Gear.State.Equipped;
			int index = currentIndex;
			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapons[i] == null)
				{
					index = i;
					break;
				}
			}
			ForceEquipAt(weapon, index);
		}

		public void ForceEquipAt(Weapon weapon, int index)
		{
			weapon.SetOwner(_character);
			weapon.gameObject.SetActive(true);
			weapon.state = Characters.Gear.Gear.State.Equipped;
			Weapon weapon2 = weapons[index];
			EquipAt(weapon, index);
			if (weapon2 != null)
			{
				UnityEngine.Object.Destroy(weapon2.gameObject);
			}
		}

		public void Polymorph(Weapon target)
		{
			Unpolymorph();
			polymorphWeapon = target;
			current.gameObject.SetActive(false);
			EndUse(current);
			polymorphWeapon.SetOwner(_character);
			polymorphWeapon.gameObject.SetActive(true);
			polymorphWeapon.state = Characters.Gear.Gear.State.Equipped;
			polymorphWeapon.transform.parent = _character.@base;
			polymorphWeapon.transform.localPosition = Vector3.zero;
			polymorphWeapon.transform.localScale = Vector3.one;
			StartUse(polymorphWeapon);
			this.onChanged?.Invoke(current, polymorphWeapon);
		}

		public void Unpolymorph()
		{
			if (!(polymorphWeapon == null))
			{
				polymorphWeapon.gameObject.SetActive(false);
				EndUse(polymorphWeapon);
				polymorphWeapon.transform.parent = null;
				StartUse(current);
				this.onChanged?.Invoke(polymorphWeapon, current);
				polymorphWeapon = null;
			}
		}

		public void ReduceSwapCooldown(float second)
		{
			_remainCooldown -= second;
			if (_remainCooldown < 0f)
			{
				_remainCooldown = 0f;
			}
		}

		public int GetCountByCategory(Weapon.Category category)
		{
			int num = 0;
			Weapon[] array = weapons;
			foreach (Weapon weapon in array)
			{
				if (!(weapon == null) && weapon.category == category)
				{
					num++;
				}
			}
			return num;
		}

		public int GetCountByRarity(Rarity rarity)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			Weapon[] array = weapons;
			foreach (Weapon weapon in array)
			{
				if (!(weapon == null) && weapon.rarity == rarity)
				{
					num++;
				}
			}
			return num;
		}

		public void UpgradeCurrentWeapon()
		{
			StartCoroutine(CUpgradeCurrentWeapon());
		}

		public IEnumerator CUpgradeCurrentWeapon()
		{
			current.UnapplyAllSkillChanges();
			string[] array = current.currentSkills.Select((SkillInfo skill) => skill.key).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				int num = text.IndexOf('_');
				if (num >= 0)
				{
					array[i] = text.Substring(0, num);
				}
			}
			yield return CWaitForUpgrade(array);
		}

		private IEnumerator CWaitForUpgrade(string[] skillKeys)
		{
			Resource.Request<Weapon> request = current.nextLevelReference.LoadAsync();
			while (!request.isDone)
			{
				yield return null;
			}
			Weapon weapon = Singleton<Service>.Instance.levelManager.DropWeapon(request.asset, base.transform.position);
			current.destructible = false;
			weapon.SetSkills(skillKeys);
			ForceEquipAt(weapon, currentIndex);
		}

		public bool Has(string weaponKey)
		{
			for (int i = 0; i < weapons.Length; i++)
			{
				if (!(weapons[i] == null) && weapons[i].name.Equals(weaponKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}
