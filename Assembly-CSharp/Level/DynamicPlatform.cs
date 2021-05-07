using System.Collections.Generic;
using Characters.Movements;
using UnityEngine;

namespace Level
{
	public class DynamicPlatform : MonoBehaviour
	{
		private readonly List<CharacterController2D> _controllers = new List<CharacterController2D>();

		private Vector3 _positionBefore;

		private void Awake()
		{
			_positionBefore = base.transform.position;
		}

		private void Update()
		{
			if (_controllers.Count > 0)
			{
				Vector3 vector = base.transform.position - _positionBefore;
				Physics2D.SyncTransforms();
				for (int i = 0; i < _controllers.Count; i++)
				{
					_controllers[i].Move(vector);
				}
			}
			_positionBefore = base.transform.position;
		}

		public void Attach(CharacterController2D controller)
		{
			_controllers.Add(controller);
		}

		public bool Detach(CharacterController2D controller)
		{
			return _controllers.Remove(controller);
		}
	}
}
