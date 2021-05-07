using System;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class SpreadOperationRunner : CharacterOperation
	{
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private Transform _origin;

		[SerializeField]
		private CustomFloat _count;

		[SerializeField]
		private CustomFloat _distance;

		[SerializeField]
		private LayerMask _groundMask;

		private static readonly NonAllocCaster _nonAllocCaster;

		static SpreadOperationRunner()
		{
			_nonAllocCaster = new NonAllocCaster(1);
		}

		public override void Run(Character owner)
		{
			float z = _origin.transform.rotation.eulerAngles.z;
			if (!0f.Equals(z))
			{
				if (!90f.Equals(z))
				{
					if (!180f.Equals(z))
					{
						if (270f.Equals(z))
						{
							SpreadLeft(_origin.transform.position, owner);
						}
					}
					else
					{
						SpreadUp(_origin.transform.position, owner);
					}
				}
				else
				{
					SpreadRight(_origin.transform.position, owner);
				}
			}
			else
			{
				SpreadDown(_origin.transform.position, owner);
			}
		}

		private ValueTuple<bool, RaycastHit2D> TryRayCast(Vector2 origin, Vector2 direction, float distance)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, direction, distance).results;
			if (results.Count > 0)
			{
				return new ValueTuple<bool, RaycastHit2D>(true, results[0]);
			}
			return new ValueTuple<bool, RaycastHit2D>(false, default(RaycastHit2D));
		}

		private bool TrySpread(Vector2 origin, Vector2 direction, Vector2 groundDirection, int count, float rotation, Character owner)
		{
			Vector2 origin2 = origin;
			float x = direction.x;
			if (!1f.Equals(x))
			{
				if ((-1f).Equals(x))
				{
					origin2 = new Vector2(origin.x, origin.y + 0.5f);
				}
			}
			else
			{
				origin2 = new Vector2(origin.x, origin.y + 0.5f);
			}
			x = direction.y;
			if (!1f.Equals(x))
			{
				if ((-1f).Equals(x))
				{
					origin2 = new Vector2(origin.x + 0.5f, origin.y);
				}
			}
			else
			{
				origin2 = new Vector2(origin.x + 0.5f, origin.y);
			}
			float value = _distance.value;
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			if (_nonAllocCaster.RayCast(origin2, direction, value * (float)count).results.Count > 0)
			{
				return false;
			}
			Vector2 origin3 = origin;
			x = direction.x;
			if (!1f.Equals(x))
			{
				if ((-1f).Equals(x))
				{
					origin3 = new Vector2(origin.x - value * (float)count, origin.y + 1f);
				}
			}
			else
			{
				origin3 = new Vector2(origin.x + value * (float)count, origin.y + 1f);
			}
			x = direction.y;
			if (!1f.Equals(x))
			{
				if ((-1f).Equals(x))
				{
					origin3 = new Vector2(origin.x + 1f, origin.y - value * (float)count);
				}
			}
			else
			{
				origin3 = new Vector2(origin.x + 1f, origin.y + value * (float)count);
			}
			ValueTuple<bool, RaycastHit2D> valueTuple = TryRayCast(origin3, groundDirection, 2f);
			bool item = valueTuple.Item1;
			RaycastHit2D item2 = valueTuple.Item2;
			if (!item)
			{
				return false;
			}
			OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
			operationInfos.transform.SetPositionAndRotation(item2.point, Quaternion.Euler(0f, 0f, rotation));
			operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
			operationInfos.Run(owner);
			return true;
		}

		private void SpreadDown(Vector2 origin, Character owner)
		{
			float value = _count.value;
			for (int i = 1; (float)i <= value && TrySpread(origin, Vector2.right, Vector2.down, i, 0f, owner); i++)
			{
			}
			for (int j = 1; (float)j <= value && TrySpread(origin, Vector2.left, Vector2.down, j, 0f, owner); j++)
			{
			}
		}

		private void SpreadUp(Vector2 origin, Character owner)
		{
			float value = _count.value;
			for (int i = 1; (float)i <= value && TrySpread(origin, Vector2.right, Vector2.up, i, 180f, owner); i++)
			{
			}
			for (int j = 1; (float)j <= value && TrySpread(origin, Vector2.left, Vector2.up, j, 180f, owner); j++)
			{
			}
		}

		private void SpreadRight(Vector2 origin, Character owner)
		{
			float value = _count.value;
			for (int i = 1; (float)i <= value && TrySpread(origin, Vector2.up, Vector2.right, i, 90f, owner); i++)
			{
			}
			for (int j = 1; (float)j <= value && TrySpread(origin, Vector2.down, Vector2.right, j, 90f, owner); j++)
			{
			}
		}

		private void SpreadLeft(Vector2 origin, Character owner)
		{
			float value = _count.value;
			for (int i = 1; (float)i <= value && TrySpread(origin, Vector2.up, Vector2.left, i, 270f, owner); i++)
			{
			}
			for (int j = 1; (float)j <= value && TrySpread(origin, Vector2.down, Vector2.left, j, 270f, owner); j++)
			{
			}
		}
	}
}
