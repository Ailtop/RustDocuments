using Characters;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Level.Traps
{
	[ExecuteAlways]
	public class ThornTrap : Trap
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
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _operationInfos;

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
			_operationInfos.Initialize();
			_operationInfos.Run(_character);
		}

		private void Update()
		{
		}
	}
}
