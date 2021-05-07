using FX;
using Level;
using UnityEngine;

public class SpawnObjectAtMapCleared : MonoBehaviour
{
	[SerializeField]
	private EnemyWave _enemyWave;

	[SerializeField]
	private EffectInfo _effect;

	[SerializeField]
	private GameObject _gameObject;

	private void Start()
	{
		_enemyWave.onClear += Spawn;
	}

	private void Spawn()
	{
		_effect.Spawn(_gameObject.transform.position);
		_gameObject.SetActive(true);
		_enemyWave.onClear -= Spawn;
	}

	private void OnDestroy()
	{
		_enemyWave.onClear -= Spawn;
	}
}
