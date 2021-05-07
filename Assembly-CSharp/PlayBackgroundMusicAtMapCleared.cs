using Level;
using Services;
using Singletons;
using UnityEngine;

public class PlayBackgroundMusicAtMapCleared : MonoBehaviour
{
	[SerializeField]
	private EnemyWave _enemyWave;

	private void Start()
	{
		_enemyWave.onClear += Spawn;
	}

	private void Spawn()
	{
		PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(Singleton<Service>.Instance.levelManager.currentChapter.currentStage.music);
		_enemyWave.onClear -= Spawn;
	}

	private void OnDestroy()
	{
		_enemyWave.onClear -= Spawn;
	}
}
