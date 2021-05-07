using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hud
{
	public class HeadupDisplay : MonoBehaviour
	{
		private Character _character;

		private WeaponInventory _weaponInventory;

		private ItemInventory _itemInventory;

		private QuintessenceInventory _quintessenceInventory;

		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private GameObject _rightBottomWithMinimap;

		[SerializeField]
		private GameObject _rightBottomWithoutMinimap;

		[SerializeField]
		private AbilityIconDisplay _abilityIconDisplay;

		[SerializeField]
		private CharacterHealthBar _healthBar;

		[SerializeField]
		private HealthValue _healthValue;

		[SerializeField]
		private GaugeBar _gaugeBar;

		[SerializeField]
		private BossHealthbarController _bossHealthBar;

		[SerializeField]
		private Image _currentWeapon;

		[SerializeField]
		private Image _nextWeapon;

		[SerializeField]
		private Image _changeWeaponCooldown;

		[SerializeField]
		private ActionIcon[] _skills;

		[SerializeField]
		private ActionIcon[] _subskills;

		[SerializeField]
		private IconWithCooldown _quintessence;

		[SerializeField]
		private Animator _swapReadyEffect;

		private CoroutineReference _cPlaySwapReadyEffectReference;

		public AbilityIconDisplay abilityIconDisplay => _abilityIconDisplay;

		public BossHealthbarController bossHealthBar => _bossHealthBar;

		public bool visible
		{
			get
			{
				return _container.activeSelf;
			}
			set
			{
				_container.SetActive(value);
			}
		}

		public bool minimapVisible
		{
			get
			{
				return _rightBottomWithMinimap.activeSelf;
			}
			set
			{
				_rightBottomWithMinimap.SetActive(value);
				_rightBottomWithoutMinimap.SetActive(!value);
			}
		}

		public void Initialize(Character player)
		{
			_character = player;
			_weaponInventory = player.GetComponent<WeaponInventory>();
			_itemInventory = player.GetComponent<ItemInventory>();
			_quintessenceInventory = player.GetComponent<QuintessenceInventory>();
			_abilityIconDisplay.Initialize(player);
			_healthBar.Initialize(player);
			_healthValue.Initialize(player.health, player.health.shield);
			_weaponInventory.onSwap += UpdateGauge;
			_weaponInventory.onChanged += OnWeaponChange;
			_weaponInventory.onSwapReady += SpawnSwapReadyEffect;
		}

		private void SpawnSwapReadyEffect()
		{
			if (base.isActiveAndEnabled)
			{
				_cPlaySwapReadyEffectReference.Stop();
				_cPlaySwapReadyEffectReference = this.StartCoroutineWithReference(CPlaySwapReadyEffect());
			}
		}

		private IEnumerator CPlaySwapReadyEffect()
		{
			_swapReadyEffect.gameObject.SetActive(true);
			_swapReadyEffect.Play(0, 0, 0f);
			yield return Chronometer.global.WaitForSeconds(_swapReadyEffect.GetCurrentAnimatorStateInfo(0).length);
			_swapReadyEffect.gameObject.SetActive(false);
		}

		private void OnWeaponChange(Weapon old, Weapon @new)
		{
			UpdateGauge();
		}

		private void UpdateGauge()
		{
			_gaugeBar.gauge = _weaponInventory.polymorphOrCurrent.gauge;
		}

		private void SetActive(GameObject gameObject, bool value)
		{
			if (gameObject.activeSelf != value)
			{
				gameObject.SetActive(value);
			}
		}

		private void OnDisable()
		{
			_swapReadyEffect.gameObject.SetActive(false);
		}

		private void Update()
		{
			if (_character == null)
			{
				return;
			}
			_currentWeapon.sprite = _weaponInventory.polymorphOrCurrent.mainIcon;
			Weapon next = _weaponInventory.next;
			if (next == null)
			{
				SetActive(_nextWeapon.transform.parent.gameObject, false);
				_changeWeaponCooldown.fillAmount = 0f;
				for (int i = 0; i < _subskills.Length; i++)
				{
					SetActive(_subskills[i].gameObject, false);
				}
			}
			else
			{
				SetActive(_nextWeapon.transform.parent.gameObject, true);
				_nextWeapon.sprite = next.subIcon;
				_nextWeapon.preserveAspect = true;
				_changeWeaponCooldown.fillAmount = _weaponInventory.reaminCooldownPercent;
				List<SkillInfo> currentSkills = _weaponInventory.next.currentSkills;
				for (int j = 0; j < _subskills.Length; j++)
				{
					ActionIcon actionIcon = _subskills[j];
					if (j >= currentSkills.Count)
					{
						SetActive(actionIcon.gameObject, false);
						continue;
					}
					SkillInfo skillInfo = currentSkills[j];
					SetActive(actionIcon.gameObject, true);
					actionIcon.icon.sprite = skillInfo.cachedIcon;
					actionIcon.icon.preserveAspect = true;
					actionIcon.action = skillInfo.action;
					actionIcon.cooldown = skillInfo.action.cooldown;
				}
			}
			List<SkillInfo> currentSkills2 = _weaponInventory.polymorphOrCurrent.currentSkills;
			for (int k = 0; k < _skills.Length; k++)
			{
				ActionIcon actionIcon2 = _skills[k];
				if (k >= currentSkills2.Count)
				{
					SetActive(actionIcon2.gameObject, false);
					continue;
				}
				SkillInfo skillInfo2 = currentSkills2[k];
				SetActive(actionIcon2.gameObject, true);
				actionIcon2.icon.sprite = skillInfo2.cachedIcon;
				actionIcon2.icon.preserveAspect = true;
				actionIcon2.action = skillInfo2.action;
				actionIcon2.cooldown = skillInfo2.action.cooldown;
			}
			if (_quintessenceInventory.items[0] == null)
			{
				SetActive(_quintessence.gameObject, false);
				return;
			}
			SetActive(_quintessence.gameObject, true);
			_003CUpdate_003Eg__SetQuintessenceInfo_007C37_0(_quintessence, _quintessenceInventory.items[0]);
		}
	}
}
