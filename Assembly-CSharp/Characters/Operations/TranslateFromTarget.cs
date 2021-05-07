using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class TranslateFromTarget : CharacterOperation
	{
		[SerializeField]
		private Transform _transform;

		[SerializeField]
		private AIController _aIController;

		[SerializeField]
		[Range(0f, 10f)]
		private float _offsetY;

		[SerializeField]
		[Range(0f, 10f)]
		private float _distributionX;

		public override void Run(Character owner)
		{
			Vector3 position = _aIController.target.transform.position;
			bool flag = owner.transform.position.x < position.x;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, flag ? Vector2.left : Vector2.right, 7f, 256);
			Vector3 position2 = Vector3.zero;
			if (!raycastHit2D)
			{
				position2 = new Vector3(flag ? (position.x - _distributionX) : (position.x + _distributionX), position.y - _offsetY, 0f);
			}
			else
			{
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(base.transform.position, (!flag) ? Vector2.left : Vector2.right, 7f, 256);
				position2 = ((!raycastHit2D2) ? new Vector3((!flag) ? (position.x - _distributionX) : (position.x + _distributionX), position.y - _offsetY, 0f) : ((!(raycastHit2D.distance > raycastHit2D2.distance)) ? new Vector3(flag ? (position.x - raycastHit2D2.distance) : (position.x + raycastHit2D2.distance), position.y - _offsetY, 0f) : new Vector3(flag ? (position.x - raycastHit2D.distance) : (position.x + raycastHit2D.distance), position.y - _offsetY, 0f)));
			}
			_transform.position = position2;
			if (position.x > _transform.position.x)
			{
				_transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else
			{
				_transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
	}
}
