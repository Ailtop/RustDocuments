using System;
using System.Runtime.CompilerServices;
using Level.BlackMarket;
using Level.Npc.FieldNpcs;
using UnityEngine;

namespace Level
{
	public abstract class IStageInfo : ScriptableObject
	{
		public Resource.MapReference[] maps;

		[SerializeField]
		private AudioClip _music;

		[SerializeField]
		private float _healthMultiplier = 1f;

		[SerializeField]
		private float _adventurerHealthMultiplier = 1f;

		[SerializeField]
		private float _adventurerAttackDamageMultiplier = 1f;

		[SerializeField]
		private float _adventurerCastingBreakDamageMultiplier = 1f;

		[SerializeField]
		[MinMaxSlider(1f, 99f)]
		private Vector2Int _adventurerLevel = new Vector2Int(1, 99);

		[SerializeField]
		private Vector2Int _goldrewardAmount = new Vector2Int(90, 110);

		[SerializeField]
		private RarityPossibilities _gearPossibilities;

		[SerializeField]
		private Level.BlackMarket.SettingsByStage _marketSettings;

		[SerializeField]
		private Level.Npc.FieldNpcs.SettingsByStage _fieldNpcSettings;

		[SerializeField]
		private CurrencyRangeByRarity _goldRangeByRarity;

		[SerializeField]
		private CurrencyRangeByRarity _darkQuartzRangeByRarity;

		[SerializeField]
		private CurrencyRangeByRarity _boneRangeByRarity;

		public AudioClip music => _music;

		public float healthMultiplier => _healthMultiplier;

		public float adventurerHealthMultiplier => _adventurerHealthMultiplier;

		public float adventurerAttackDamageMultiplier => _adventurerAttackDamageMultiplier;

		public float adventurerCastingBreakDamageMultiplier => _adventurerCastingBreakDamageMultiplier;

		public Vector2Int adventurerLevel => _adventurerLevel;

		public Vector2Int goldrewardAmount => _goldrewardAmount;

		public RarityPossibilities gearPossibilities => _gearPossibilities;

		public Level.BlackMarket.SettingsByStage marketSettings => _marketSettings;

		public Level.Npc.FieldNpcs.SettingsByStage fieldNpcSettings => _fieldNpcSettings;

		public CurrencyRangeByRarity goldRangeByRarity => _goldRangeByRarity;

		public CurrencyRangeByRarity darkQuartzRangeByRarity => _darkQuartzRangeByRarity;

		public CurrencyRangeByRarity boneRangeByRarity => _boneRangeByRarity;

		[TupleElementNames(new string[] { "node1", "node2" })]
		public abstract ValueTuple<PathNode, PathNode> nextMapTypes
		{
			[return: TupleElementNames(new string[] { "node1", "node2" })]
			get;
		}

		public abstract ParallaxBackground background { get; }

		public abstract void Initialize();

		public abstract void Reset();

		public abstract bool Next();

		public abstract void UpdateReferences();
	}
}
