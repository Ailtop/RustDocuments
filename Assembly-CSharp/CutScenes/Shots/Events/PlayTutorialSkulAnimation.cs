using Characters;
using Characters.Gear.Weapons;
using Characters.Player;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class PlayTutorialSkulAnimation : Event
	{
		private enum Type
		{
			Idle,
			OpenEyes,
			EquipHead,
			ScratchHead,
			Blink
		}

		[SerializeField]
		private Type _type;

		private TutorialSkul _skul;

		private WeaponInventory _inventory;

		private Character _player;

		private void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
		}

		public override void Run()
		{
			_inventory = _player.GetComponent<WeaponInventory>();
			_skul = _inventory.polymorphOrCurrent.GetComponent<TutorialSkul>();
			switch (_type)
			{
			case Type.Idle:
				_skul.idle.TryStart();
				break;
			case Type.OpenEyes:
				_skul.openEyes.TryStart();
				break;
			case Type.EquipHead:
				_skul.equipHead.TryStart();
				break;
			case Type.ScratchHead:
				_skul.scratchHead.TryStart();
				break;
			case Type.Blink:
				_skul.blink.TryStart();
				break;
			}
		}
	}
}
