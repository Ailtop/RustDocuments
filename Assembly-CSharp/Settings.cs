using Level.BlackMarket;
using UnityEngine;

public class Settings : ScriptableObject
{
	[Header("Container")]
	[SerializeField]
	private RarityPossibilities _commonContainerPossibilities;

	[SerializeField]
	private RarityPossibilities _rareContainerPossibilities;

	[SerializeField]
	private RarityPossibilities _uniqueContainerPossibilities;

	[SerializeField]
	private RarityPossibilities _legendaryContainerPossibilities;

	[Header("Prop Gold")]
	[SerializeField]
	private GoldPossibility _smallPropGoldPossibility;

	[SerializeField]
	private GoldPossibility _largePropGoldPossibility;

	[Header("Head")]
	[SerializeField]
	private RarityPrices _bonesByDiscard;

	[SerializeField]
	private RarityPrices _bonesToUpgrade;

	[Header("Blackmarket")]
	[SerializeField]
	private GlobalSettings _marketSettings;

	private static Settings _instance;

	public EnumArray<Rarity, RarityPossibilities> containerPossibilities { get; private set; }

	public GoldPossibility smallPropGoldPossibility => _smallPropGoldPossibility;

	public GoldPossibility largePropGoldPossibility => _largePropGoldPossibility;

	public RarityPrices bonesByDiscard => _bonesByDiscard;

	public RarityPrices bonesToUpgrade => _bonesToUpgrade;

	public GlobalSettings marketSettings => _marketSettings;

	public static Settings instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<Settings>("Settings");
				_instance.Initialize();
			}
			return _instance;
		}
	}

	public void Initialize()
	{
		containerPossibilities = new EnumArray<Rarity, RarityPossibilities>();
		containerPossibilities[(Rarity)0] = _commonContainerPossibilities;
		containerPossibilities[(Rarity)1] = _rareContainerPossibilities;
		containerPossibilities[(Rarity)2] = _uniqueContainerPossibilities;
		containerPossibilities[(Rarity)3] = _legendaryContainerPossibilities;
	}
}
