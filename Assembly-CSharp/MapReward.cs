using System;
using FX;
using Level;
using UnityEngine;

public class MapReward : MonoBehaviour
{
	[Serializable]
	public class RewardTypeGameObjectArray : EnumArray<Type, GameObject>
	{
	}

	public enum Type
	{
		None,
		Gold,
		Head,
		Item,
		Adventurer,
		Boss
	}

	[NonSerialized]
	public Type type;

	[SerializeField]
	private SpriteRenderer _preview;

	[SerializeField]
	private RewardTypeGameObjectArray _rewardPrefabs;

	[SerializeField]
	private EffectInfo _spawnEffect;

	[SerializeField]
	private Transform _spawnEffectPosition;

	private GameObject _reward;

	public bool hasReward => _rewardPrefabs[type] != null;

	public bool activated { get; set; }

	public event Action onLoot;

	private void Awake()
	{
		UnityEngine.Object.Destroy(_preview);
	}

	public void LoadReward()
	{
		GameObject gameObject = _rewardPrefabs[type];
		if (gameObject == null)
		{
			this.onLoot?.Invoke();
			return;
		}
		_reward = UnityEngine.Object.Instantiate(gameObject, base.transform.position, Quaternion.identity, base.transform);
		_reward.gameObject.SetActive(false);
		_reward.GetComponent<ILootable>().onLoot += delegate
		{
			this.onLoot?.Invoke();
		};
	}

	public bool Activate()
	{
		activated = true;
		if (_reward == null)
		{
			return false;
		}
		_reward.gameObject.SetActive(true);
		_reward.GetComponent<ILootable>().Activate();
		_spawnEffect.Spawn(_spawnEffectPosition.position);
		return true;
	}
}
