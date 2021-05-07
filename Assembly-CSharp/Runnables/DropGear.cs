using System.Collections;
using Characters.Gear;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Runnables
{
	public sealed class DropGear : Runnable
	{
		[SerializeField]
		private GearPossibilities _gearPossibilities;

		[SerializeField]
		private RarityPossibilities _rarityPossibilities;

		private Resource.GearReference _gearReference;

		private Resource.Request<Gear> _gearRequest;

		public override void Run()
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
			Singleton<Service>.Instance.levelManager.DropGear(_gearRequest.asset, base.transform.position);
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
	}
}
