using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.Gear;
using Services;
using Singletons;

namespace Level.Npc.Prophecies
{
	public abstract class Prophecy
	{
		protected readonly string stringKeyBase = "prophecy";

		public static readonly TrickyPassage trickyPassage;

		public static readonly RamiasGift ramiasGift;

		public static readonly AdventureTime adventureTime;

		public static readonly ReleasedTree releasedTree;

		public static readonly ViolentBlow violentBlow;

		public static readonly PeddleInForest peddleInForest;

		public static readonly PlainVanilla plainVanilla;

		public static readonly Prophecy[] prophecies;

		public static readonly Dictionary<string, Prophecy> prophecyDictionary;

		public static readonly EnumArray<Rarity, Prophecy[]> rarityProphecies;

		public static Prophecy prophecyFromDruid;

		public readonly string key;

		public readonly Rarity rarity;

		public Character _owner;

		public bool fulfiled;

		public bool activated { get; protected set; }

		public bool canFulfil
		{
			get
			{
				if (activated)
				{
					return !fulfiled;
				}
				return false;
			}
		}

		public string name => Lingua.GetLocalizedString(stringKeyBase + "/" + key);

		public string description => Lingua.GetLocalizedString(stringKeyBase + "/" + key + "/desc");

		public string script => Lingua.GetLocalizedString(stringKeyBase + "/" + key + "/script");

		static Prophecy()
		{
			prophecies = new Prophecy[7]
			{
				trickyPassage = new TrickyPassage("trickeyPassage", (Rarity)0),
				ramiasGift = new RamiasGift("ramiasGift", (Rarity)0),
				adventureTime = new AdventureTime("adventureTime", (Rarity)0),
				releasedTree = new ReleasedTree("releasedTree", (Rarity)1),
				violentBlow = new ViolentBlow("violentBlow", (Rarity)1),
				peddleInForest = new PeddleInForest("peddleInForest", (Rarity)1),
				plainVanilla = new PlainVanilla("plainVanilla", (Rarity)3)
			};
			rarityProphecies = new EnumArray<Rarity, Prophecy[]>();
		}

		public static void ResetAll()
		{
			Prophecy[] array = prophecies;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Deactivate();
			}
		}

		protected Prophecy(string key, Rarity rarity)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			this.key = key;
			this.rarity = rarity;
		}

		public void Fulfil()
		{
			if (canFulfil)
			{
				fulfiled = true;
				activated = false;
				GetReward();
			}
		}

		protected abstract void GetReward();

		protected abstract void Reset();

		protected abstract void OnActivate();

		protected abstract void OnDeactivate();

		public void Activate(Character owner)
		{
			if (!activated)
			{
				_owner = owner;
				activated = true;
				Reset();
				OnActivate();
			}
		}

		public void Deactivate()
		{
			if (activated)
			{
				activated = false;
				OnDeactivate();
			}
		}

		protected void DropGear()
		{
			Singleton<Service>.Instance.levelManager.StartCoroutine(CDropGear());
		}

		protected IEnumerator CDropGear()
		{
			Resource.Request<Gear> request = Singleton<Service>.Instance.gearManager.GetGearToTake(rarity).LoadAsync();
			if (!request.isDone)
			{
				yield return null;
			}
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			levelManager.DropGear(request.asset, levelManager.player.transform.position);
		}
	}
}
