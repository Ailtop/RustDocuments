using System.Collections;
using System.Collections.Generic;

namespace Characters.AI.Hero
{
	public class ComboSystem
	{
		private int _maxCount = 2;

		private int _currentCount;

		private List<IComboable> _combos;

		private int[] _comboChances = new int[5] { 60, 30, 10, 1, 0 };

		public ComboSystem()
		{
			_combos = new List<IComboable>();
		}

		public ComboSystem AddComboPattern(IComboable comboable)
		{
			_combos.Add(comboable);
			return this;
		}

		public bool TryComboAttack()
		{
			if (_currentCount >= _maxCount)
			{
				return false;
			}
			return true;
		}

		public IEnumerator CNext(AIController controller)
		{
			yield return _combos[_currentCount++].CTryContinuedCombo(controller, this);
		}

		public void Start()
		{
			_combos.Shuffle();
		}

		public void Clear()
		{
			_currentCount = 0;
		}
	}
}
