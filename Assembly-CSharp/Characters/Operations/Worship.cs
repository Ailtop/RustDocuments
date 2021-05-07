using UnityEngine;

namespace Characters.Operations
{
	public class Worship : CharacterOperation
	{
		[SerializeField]
		private int _firedAtOnce;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _commonOperations;

		[SerializeField]
		private OperationInfos[] _operations = new OperationInfos[4];

		private int _current;

		private void Awake()
		{
			for (int i = 0; i < _operations.Length; i++)
			{
				_operations[i].Initialize();
			}
		}

		public override void Run(Character owner)
		{
			if (_current >= _operations.Length || _current == 0)
			{
				_current = 0;
				_operations.Shuffle();
			}
			int num = _current;
			int num2 = 0;
			while (num < _operations.Length && num2 < _firedAtOnce)
			{
				_commonOperations.Run(owner);
				_operations[num].gameObject.SetActive(true);
				_operations[num].Run(owner);
				num++;
				num2++;
				_current = num;
			}
		}
	}
}
