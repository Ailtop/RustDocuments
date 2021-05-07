using Characters;
using Characters.AI.Adventurer;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Adventurer
{
	public class AdventurerCombatArea : MonoBehaviour
	{
		[SerializeField]
		private Commander _commander;

		[SerializeField]
		private EnemyWave _enemyWave;

		[SerializeField]
		[GetComponent]
		private BoxCollider2D _startTrigger;

		[SerializeField]
		private GameObject _leftWall;

		[SerializeField]
		private GameObject _rightWall;

		private void Awake()
		{
			_enemyWave.onClear += DisableSideWall;
			_leftWall.SetActive(false);
			_rightWall.SetActive(true);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (player.collider.bounds.min.x < _startTrigger.bounds.min.x - 0.5f)
			{
				player.movement.force += new Vector2(1f, 0f);
			}
			EnableSideWall();
			if (_commander != null)
			{
				_commander.StartIntro();
			}
		}

		private void EnableSideWall()
		{
			_startTrigger.enabled = false;
			_leftWall.SetActive(true);
			_rightWall.SetActive(true);
		}

		public void DisableSideWall()
		{
			_startTrigger.enabled = false;
			_leftWall.SetActive(false);
			_rightWall.SetActive(false);
		}
	}
}
