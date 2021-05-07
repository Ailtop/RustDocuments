using System.Collections;
using Characters.Actions;
using Characters.Operations.Attack;
using Characters.Operations.Fx;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class RisingPierce : MonoBehaviour
	{
		[SerializeField]
		private Action _ready;

		[SerializeField]
		private Action _motion;

		[SerializeField]
		private Collider2D _rangePartsOrigin;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawnAttackSign;

		[SerializeField]
		[Subcomponent(typeof(SweepAttack2))]
		private SweepAttack2 _sweepAttack;

		[Subcomponent(typeof(SpawnEffect))]
		[SerializeField]
		private SpawnEffect _sweepAttackEffect;

		[SerializeField]
		private float _attackDelay = 1f;

		[SerializeField]
		private CompositeCollider2D _range;

		[SerializeField]
		[MinMaxSlider(0f, 5f)]
		private Vector2 _startNoise;

		[SerializeField]
		[MinMaxSlider(0f, 5f)]
		private Vector2 _distanceNoise;

		[SerializeField]
		[MinMaxSlider(6f, 8f)]
		private Vector2Int _countRange;

		private float[] _cachedDistance;

		private void Awake()
		{
			Initialize();
		}

		private void Initialize()
		{
			int y = _countRange.y;
			_cachedDistance = new float[y];
			_sweepAttack.Initialize();
			for (int i = 0; i < y; i++)
			{
				GameObject obj = new GameObject();
				BoxCollider2D boxCollider2D = obj.AddComponent<BoxCollider2D>();
				boxCollider2D.size = new Vector2(_rangePartsOrigin.bounds.size.x, _rangePartsOrigin.bounds.size.y);
				boxCollider2D.offset = new Vector2(_rangePartsOrigin.offset.x, _rangePartsOrigin.offset.y);
				boxCollider2D.usedByComposite = true;
				obj.transform.SetParent(_range.transform);
				obj.SetActive(false);
			}
		}

		private void MakeAttackRange(float startX, float y, int count)
		{
			float x = _rangePartsOrigin.bounds.size.x;
			float x2 = _rangePartsOrigin.bounds.extents.x;
			for (int i = 0; i < count; i++)
			{
				Transform child = _range.transform.GetChild(i);
				float x3 = startX + (x * (float)i + x2) + (float)i * _cachedDistance[i];
				child.transform.position = new Vector2(x3, y);
			}
			_range.GenerateGeometry();
		}

		private float GetStartPointX(Character character)
		{
			float x = character.movement.controller.collisionState.lastStandingCollider.bounds.min.x;
			float num = Random.Range(_startNoise.x, _startNoise.y);
			return x + num;
		}

		public IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float startX = GetStartPointX(character);
			float sizeX = _rangePartsOrigin.bounds.size.x;
			float extentsX = _rangePartsOrigin.bounds.extents.x;
			int count = Random.Range(_countRange.x, _countRange.y);
			_ready.TryStart();
			while (_ready.running)
			{
				yield return null;
			}
			_motion.TryStart();
			for (int i = 0; i < count; i++)
			{
				float num = Random.Range(_distanceNoise.x, _distanceNoise.y);
				_cachedDistance[i] = num;
				float x = startX + (sizeX * (float)i + extentsX) + (float)i * num;
				_rangePartsOrigin.transform.position = new Vector3(x, platformBounds.max.y);
				_spawnAttackSign.Run(character);
			}
			MakeAttackRange(startX, platformBounds.max.y, count);
			yield return character.chronometer.master.WaitForSeconds(_attackDelay);
			_sweepAttack.Run(character);
			for (int j = 0; j < count; j++)
			{
				float num2 = _cachedDistance[j];
				float x2 = startX + (sizeX * (float)j + extentsX) + (float)j * num2;
				_rangePartsOrigin.transform.position = new Vector3(x2, platformBounds.max.y);
				_sweepAttackEffect.Run(character);
			}
		}
	}
}
