using System.Collections;
using Characters;
using Characters.Movements;
using UnityEngine;

namespace Level
{
	public class DroppedCorpse : DestructibleObject
	{
		private static int orderInLayerCount;

		[SerializeField]
		private Character _owner;

		[SerializeField]
		private bool _randomize;

		[SerializeField]
		private bool _collideWithTerrain = true;

		[Information("0이면 영구지속", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _fadeOutDuration;

		[SerializeField]
		private AnimationCurve _fadeOut;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		[SerializeField]
		[GetComponent]
		private BoxCollider2D _boxCollider;

		[SerializeField]
		private Vector2 _additionalForce = new Vector2(0f, 0.05f);

		public bool randomize => _randomize;

		public bool collideWithTerrain => _collideWithTerrain;

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public override Collider2D collider => _boxCollider;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
			base.transform.SetParent(Map.Instance.transform);
		}

		public void Emit()
		{
			Push push = _owner.movement.push;
			base.transform.position = _owner.transform.position;
			if (_owner.lookingDirection == Character.LookingDirection.Right)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			Initialize(push);
		}

		private void Initialize(Push push, float multiplier = 1f)
		{
			Vector2 force = Vector2.zero;
			if (push != null && !push.expired)
			{
				force = push.direction * push.totalForce;
			}
			force *= multiplier;
			Initialize(force, multiplier);
		}

		private void Initialize(Vector2 force, float multiplier = 1f)
		{
			force *= multiplier;
			_rigidbody.AddForce(force, ForceMode2D.Impulse);
			_spriteRenderer.sortingOrder = orderInLayerCount++;
			if (_duration > 0f)
			{
				StartCoroutine(CFadeOut());
			}
		}

		private IEnumerator CFadeOut()
		{
			yield return Chronometer.global.WaitForSeconds(_duration);
			base.gameObject.SetActive(false);
		}

		public override void Hit(Character from, ref Damage damage, Vector2 force)
		{
			if (Mathf.Abs(force.x) < 0.66f && Mathf.Abs(force.y) < 0.66f)
			{
				force = Random.insideUnitCircle;
			}
			force.y = Mathf.Abs(force.y);
			force *= 3f;
			force = Quaternion.AngleAxis(Random.Range(-15f, 15f), Vector3.forward) * force * Random.Range(0.8f, 1.2f);
			if (_rigidbody.IsTouchingLayers())
			{
				force += _additionalForce;
			}
			_rigidbody.AddForce(force * Random.Range(0.5f, 1f), ForceMode2D.Impulse);
			_rigidbody.AddTorque(Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);
		}
	}
}
