using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class SlamHand : MonoBehaviour
	{
		[SerializeField]
		[FrameTime]
		private float _fistSlamAttackLength = 0.16f;

		[SerializeField]
		[FrameTime]
		private float _fistSlamRecoveryLength = 0.66f;

		[SerializeField]
		[FrameTime]
		private float _attackDelayFromtargeting = 0.16f;

		[SerializeField]
		[GetComponent]
		private Collider2D _collider;

		[SerializeField]
		private AIController _ai;

		[SerializeField]
		private GameObject _monsterBody;

		[SerializeField]
		private YggdrasillElderEntCollisionDetector _collisionDetector;

		[Header("For Terrain")]
		[SerializeField]
		private SpriteRenderer _sprite;

		[SerializeField]
		private Collider2D _terrainCollider;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSlamStart;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onSlamEnd;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onRecoverySign;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onRecovery;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onEnd;

		private Coroutine _cChangeToTerrainReference;

		private Vector3 _origin;

		private Vector3 _source;

		public Vector3 destination { private get; set; }

		private void Start()
		{
			_origin = base.transform.position;
			_onSign.Initialize();
			_onSlamStart.Initialize();
			_onSlamEnd.Initialize();
			_onRecoverySign.Initialize();
			_onRecovery.Initialize();
			_onEnd.Initialize();
		}

		public void ActiavteHand()
		{
			base.gameObject.SetActive(true);
		}

		public void DeactivateHand()
		{
			base.gameObject.SetActive(false);
		}

		public void Sign()
		{
			_onSign.gameObject.SetActive(true);
			_onSign.Run(_ai.character);
		}

		public IEnumerator CSlam()
		{
			_source = _origin;
			yield return Chronometer.global.WaitForSeconds(_attackDelayFromtargeting);
			StartSlam();
			yield return CMoveTarget(_fistSlamAttackLength);
			EndSlam();
		}

		private void StartSlam()
		{
			_onSlamStart.gameObject.SetActive(true);
			_onSlamStart.Run(_ai.character);
			StartCollisionDetect();
		}

		private void EndSlam()
		{
			_onSlamEnd.gameObject.SetActive(true);
			_onSlamEnd.Run(_ai.character);
			ActivateTerrain();
			_collisionDetector.Stop();
		}

		public IEnumerator CVibrate()
		{
			float elapsedTime = 0f;
			float length = 0.45f;
			float shakeAmount = 0.25f;
			CharacterChronometer chronometer = _ai.character.chronometer;
			while (true)
			{
				_sprite.transform.localPosition = Random.insideUnitSphere * shakeAmount;
				elapsedTime += chronometer.animation.deltaTime;
				if (elapsedTime > length)
				{
					break;
				}
				yield return null;
			}
			_sprite.transform.localPosition = Vector3.zero;
		}

		public IEnumerator CRecover()
		{
			_source = base.transform.position;
			destination = _origin;
			StopCoroutine(_cChangeToTerrainReference);
			DeactivateTerrain();
			yield return CMoveTarget(_fistSlamRecoveryLength);
		}

		private IEnumerator CMoveTarget(float length)
		{
			float elapsedTime = 0f;
			CharacterChronometer chronometer = _ai.character.chronometer;
			Vector3 dest = destination;
			while (true)
			{
				Vector2 vector = Vector2.Lerp(_source, dest, elapsedTime / length);
				base.transform.position = vector;
				elapsedTime += chronometer.animation.deltaTime;
				if (elapsedTime > length)
				{
					break;
				}
				yield return null;
			}
			base.transform.position = dest;
		}

		private void ActivateTerrain()
		{
			_cChangeToTerrainReference = StartCoroutine(CChangeTerrain());
		}

		private IEnumerator CChangeTerrain()
		{
			while (true)
			{
				yield return null;
				Character character = _ai.FindClosestPlayerBody(_collider);
				_collider.enabled = true;
				if (!(character != null))
				{
					_terrainCollider.gameObject.SetActive(true);
				}
			}
		}

		private void DeactivateTerrain()
		{
			_terrainCollider.gameObject.SetActive(false);
		}

		private void StartCollisionDetect()
		{
			_collisionDetector.Initialize(_monsterBody, _collider);
			StartCoroutine(_collisionDetector.CRun(base.transform));
		}
	}
}
