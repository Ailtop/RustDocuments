using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class LightSword : MonoBehaviour
	{
		[SerializeField]
		private LightSwordStuck _stuck;

		[SerializeField]
		private LightSwordProjectile _projectile;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onDraw;

		private Character _owner;

		private CoroutineReference _moveCoroutine;

		public bool active { get; private set; }

		private void Awake()
		{
			_onDraw.Initialize();
		}

		public void Initialzie(Character owner)
		{
			_owner = owner;
			if (!(_owner == null))
			{
				_owner.health.onDiedTryCatch += Hide;
			}
		}

		public void Draw(Character owner)
		{
			_onDraw.gameObject.SetActive(true);
			_onDraw.Run(owner);
		}

		public void Fire(Character owner, Vector2 source, Vector2 destination)
		{
			float degree = AngleBetween(source, destination);
			_moveCoroutine = this.StartCoroutineWithReference(CMove(source, destination, degree));
			active = true;
		}

		private float AngleBetween(Vector2 from, Vector2 to)
		{
			Vector2 vector = to - from;
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			if (!(num < 0f))
			{
				return num;
			}
			return num + 360f;
		}

		public Vector3 GetStuckPosition()
		{
			return _stuck.transform.position;
		}

		public void Sign()
		{
			_stuck.Sign();
		}

		public void Despawn()
		{
			active = false;
			_stuck.Despawn();
		}

		private void Hide()
		{
			base.gameObject.SetActive(false);
		}

		private IEnumerator CMove(Vector2 firePosition, Vector2 destination, float degree)
		{
			yield return _projectile.CFire(firePosition, destination, degree);
			_stuck.OnStuck(_owner, destination, degree);
		}
	}
}
