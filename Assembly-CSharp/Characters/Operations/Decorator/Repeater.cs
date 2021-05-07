using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class Repeater : CharacterOperation
	{
		[SerializeField]
		private int _times;

		[SerializeField]
		private float _interval;

		[SerializeField]
		[Subcomponent]
		private CharacterOperation _toRepeat;

		private CoroutineReference _repeatCoroutineReference;

		public override void Initialize()
		{
			_toRepeat.Initialize();
		}

		private void Awake()
		{
			if (_times == 0)
			{
				_times = int.MaxValue;
			}
		}

		public override void Run(Character owner)
		{
			_003C_003Ec__DisplayClass6_0 _003C_003Ec__DisplayClass6_ = new _003C_003Ec__DisplayClass6_0();
			_003C_003Ec__DisplayClass6_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass6_.owner = owner;
			_003C_003Ec__DisplayClass6_.interval = _interval / runSpeed;
			_repeatCoroutineReference = this.StartCoroutineWithReference(_003C_003Ec__DisplayClass6_._003CRun_003Eg__CRepeat_007C0());
		}

		public override void Run(Character owner, Character target)
		{
			_003C_003Ec__DisplayClass7_0 _003C_003Ec__DisplayClass7_ = new _003C_003Ec__DisplayClass7_0();
			_003C_003Ec__DisplayClass7_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass7_.owner = owner;
			_003C_003Ec__DisplayClass7_.target = target;
			_003C_003Ec__DisplayClass7_.interval = _interval / runSpeed;
			_repeatCoroutineReference = this.StartCoroutineWithReference(_003C_003Ec__DisplayClass7_._003CRun_003Eg__CRepeat_007C0());
		}

		public override void Stop()
		{
			_toRepeat.Stop();
			_repeatCoroutineReference.Stop();
		}
	}
}
