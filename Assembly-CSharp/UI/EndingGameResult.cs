using System;
using System.Collections;
using System.Globalization;
using Characters.Controllers;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Characters.Player;
using Data;
using Scenes;
using Services;
using Singletons;
using TMPro;
using UnityEngine;

namespace UI
{
	public class EndingGameResult : MonoBehaviour
	{
		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private AnimationCurve _curve;

		[SerializeField]
		private TextMeshProUGUI _playTime;

		[SerializeField]
		private TextMeshProUGUI _deaths;

		[SerializeField]
		private TextMeshProUGUI _kills;

		[SerializeField]
		private TextMeshProUGUI _eliteKills;

		[SerializeField]
		private TextMeshProUGUI _darkcites;

		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private TextMeshProUGUI _subTitle;

		[SerializeField]
		private TextMeshProUGUI _yourEnemy;

		[SerializeField]
		private TextMeshProUGUI _stageName;

		[SerializeField]
		private Transform _gearListContainer;

		[SerializeField]
		private GearImageContainer _gearContainerPrefab;

		public bool animationFinished { get; private set; }

		private void OnEnable()
		{
			PlayerInput.blocked.Attach(this);
			Scene<GameBase>.instance.uiManager.pauseEventSystem.PushEmpty();
			Chronometer.global.AttachTimeScale(this, 0f);
			StartCoroutine(CAnimate());
			_playTime.text = new TimeSpan(0, 0, GameData.Progress.playTime).ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture);
			_deaths.text = GameData.Progress.deaths.ToString();
			_kills.text = GameData.Progress.kills.ToString();
			_eliteKills.text = GameData.Progress.eliteKills.ToString();
			_darkcites.text = GameData.Currency.darkQuartz.income.ToString();
			UpdateGearList();
		}

		private void OnDisable()
		{
			PlayerInput.blocked.Detach(this);
			Scene<GameBase>.instance.uiManager.pauseEventSystem.PopEvent();
			Chronometer.global.DetachTimeScale(this);
		}

		private void UpdateGearList()
		{
			_gearListContainer.Empty();
			Characters.Player.Inventory inventory = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory;
			WeaponInventory weapon = inventory.weapon;
			QuintessenceInventory quintessence = inventory.quintessence;
			ItemInventory item = inventory.item;
			for (int i = 0; i < weapon.weapons.Length; i++)
			{
				Weapon weapon2 = weapon.weapons[i];
				if (weapon2 != null)
				{
					GearImageContainer gearImageContainer = UnityEngine.Object.Instantiate(_gearContainerPrefab, _gearListContainer);
					gearImageContainer.image.sprite = weapon2.icon;
					gearImageContainer.image.SetNativeSize();
				}
			}
			for (int j = 0; j < quintessence.items.Count; j++)
			{
				Quintessence quintessence2 = quintessence.items[j];
				if (quintessence2 != null)
				{
					GearImageContainer gearImageContainer2 = UnityEngine.Object.Instantiate(_gearContainerPrefab, _gearListContainer);
					gearImageContainer2.image.sprite = quintessence2.icon;
					gearImageContainer2.image.SetNativeSize();
				}
			}
			for (int k = 0; k < item.items.Count; k++)
			{
				Item item2 = item.items[k];
				if (item2 != null)
				{
					GearImageContainer gearImageContainer3 = UnityEngine.Object.Instantiate(_gearContainerPrefab, _gearListContainer);
					gearImageContainer3.image.sprite = item2.icon;
					gearImageContainer3.image.SetNativeSize();
				}
			}
		}

		private IEnumerator CAnimate()
		{
			animationFinished = false;
			float time = 0f;
			Vector3 targetPosition = _container.transform.position;
			Vector3 position = targetPosition;
			position.y += 200f;
			for (; time < 1f; time += Time.unscaledDeltaTime)
			{
				_container.transform.position = Vector3.LerpUnclamped(position, targetPosition, _curve.Evaluate(time));
				yield return null;
			}
			_container.transform.position = targetPosition;
			animationFinished = true;
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}
	}
}
