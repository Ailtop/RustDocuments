using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class Repeater : Operation
	{
		[SerializeField]
		private int _times;

		[SerializeField]
		private float _interval;

		[SerializeField]
		[Subcomponent]
		private Operation _toRepeat;

		private CoroutineReference _repeatCoroutineReference;

		private void Awake()
		{
			if (_times == 0)
			{
				_times = int.MaxValue;
			}
		}

		public override void Run(Projectile projectile)
		{
			_003C_003Ec__DisplayClass5_0 _003C_003Ec__DisplayClass5_ = new _003C_003Ec__DisplayClass5_0();
			_003C_003Ec__DisplayClass5_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass5_.projectile = projectile;
			_003C_003Ec__DisplayClass5_.interval = _interval;
			_repeatCoroutineReference = this.StartCoroutineWithReference(_003C_003Ec__DisplayClass5_._003CRun_003Eg__CRepeat_007C0());
		}
	}
}
