using Characters;
using Characters.Operations.Attack;
using UnityEngine;

namespace Level.Traps
{
	[ExecuteAlways]
	public class PoisonSwamp : Trap
	{
		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Character _character;

		[SerializeField]
		private BoxCollider2D _collider;

		[SerializeField]
		private int _size = 1;

		[SerializeField]
		private SweepAttack _sweepAttack;

		[SerializeField]
		private SweepAttack _sweepAttackForPoison;

		private void SetSize()
		{
			Vector2 size = _spriteRenderer.size;
			size.x = _size * 2;
			_spriteRenderer.size = size;
			Vector2 size2 = _collider.size;
			size2.x = (float)(_size * 2) - 1.2f;
			_collider.size = size2;
		}

		private void Awake()
		{
			SetSize();
			_sweepAttack.Initialize();
			_sweepAttackForPoison.Initialize();
			_sweepAttack.Run(_character);
			_sweepAttackForPoison.Run(_character);
		}

		private void Update()
		{
		}
	}
}
