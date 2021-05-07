using System.Collections;
using Characters;
using Characters.Gear;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
	public sealed class GearReward : MonoBehaviour
	{
		[SerializeField]
		private Transform _dropPoint;

		[SerializeField]
		private bool _hasMovements;

		[SerializeField]
		private GearPossibilities _gearPossibilities;

		[SerializeField]
		private RarityPossibilities _rarityPossibilities;

		[SerializeField]
		private UnityEvent _onDrop;

		[SerializeField]
		private UnityEvent _onDestroy;

		[SerializeField]
		private UnityEvent _onLoot;

		private Resource.GearReference _gearReference;

		private Resource.Request<Gear> _gearRequest;

		private Gear _droppedGear;

		public void Lock()
		{
			_droppedGear.destructible = false;
			_droppedGear.lootable = false;
		}

		public void Unlock()
		{
			_droppedGear.destructible = true;
			_droppedGear.lootable = true;
		}

		public void Drop()
		{
			StartCoroutine("CDrop");
		}

		private IEnumerator CDrop()
		{
			Load();
			while (!_gearRequest.isDone)
			{
				yield return null;
			}
			_droppedGear = Singleton<Service>.Instance.levelManager.DropGear(_gearRequest.asset, _dropPoint.position);
			if (!_hasMovements)
			{
				_droppedGear.dropped.dropMovement.Stop();
			}
			_onDrop?.Invoke();
			_droppedGear.dropped.onLoot += OnGearLoot;
			_droppedGear.dropped.onDestroy += OnGearDestroy;
		}

		private void Load()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			Rarity key = _rarityPossibilities.Evaluate();
			Gear.Type? type = _gearPossibilities.Evaluate();
			do
			{
				Rarity rarity = Settings.instance.containerPossibilities[key].Evaluate();
				switch (type)
				{
				case Gear.Type.Weapon:
					_gearReference = Singleton<Service>.Instance.gearManager.GetWeaponToTake(rarity);
					break;
				case Gear.Type.Item:
					_gearReference = Singleton<Service>.Instance.gearManager.GetItemToTake(rarity);
					break;
				case Gear.Type.Quintessence:
					_gearReference = Singleton<Service>.Instance.gearManager.GetQuintessenceToTake(rarity);
					break;
				}
			}
			while (_gearReference == null);
			_gearRequest = _gearReference.LoadAsync();
		}

		private void OnGearLoot(Character character)
		{
			_droppedGear.dropped.onLoot -= OnGearLoot;
			_onLoot?.Invoke();
		}

		private void OnGearDestroy(Character character)
		{
			_droppedGear.dropped.onDestroy -= OnGearDestroy;
			_onDestroy?.Invoke();
		}
	}
}
