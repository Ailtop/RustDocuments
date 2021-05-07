using System.Collections;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class SaintField : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		private float _duration = 10f;

		[SerializeField]
		private GameObject _leftPillarOfLight;

		[SerializeField]
		private GameObject _rightPillarOfLight;

		[SerializeField]
		private Transform _fireTransform;

		[SerializeField]
		private GiganticSaintSword _sword;

		[SerializeField]
		private float _height;

		private Character _player;

		public bool isStuck => _sword.isStuck;

		private void Start()
		{
			_player = Singleton<Service>.Instance.levelManager.player;
			_sword.OnStuck += ActivePillarOfLight;
			_sword.OnStuck += delegate
			{
				StartCoroutine(CExpire());
			};
			_owner.health.onDiedTryCatch += DeactivePillarOfLight;
		}

		public void DropGiganticSaintSword()
		{
			float y = _player.movement.controller.collisionState.lastStandingCollider.bounds.max.y;
			_sword.Fire(_fireTransform.position, y);
		}

		private void ActivePillarOfLight()
		{
			_leftPillarOfLight.SetActive(true);
			_rightPillarOfLight.SetActive(true);
		}

		public void DeactivePillarOfLight()
		{
			_leftPillarOfLight.SetActive(false);
			_rightPillarOfLight.SetActive(false);
			_sword.Despawn();
		}

		private IEnumerator CExpire()
		{
			float elapsed = 0f;
			while (elapsed < _duration)
			{
				yield return null;
				elapsed += Chronometer.global.deltaTime;
				if (!ContainsPlayer())
				{
					break;
				}
			}
			DeactivePillarOfLight();
		}

		private bool ContainsPlayer()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (_leftPillarOfLight.transform.position.x > player.transform.position.x)
			{
				return false;
			}
			if (_rightPillarOfLight.transform.position.x < player.transform.position.x)
			{
				return false;
			}
			return true;
		}
	}
}
