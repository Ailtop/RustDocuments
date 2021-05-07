using System.Collections;
using System.Linq;
using Characters.AI.Behaviours.Attacks;
using Level.Chapter4;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class Purification : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _ready;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveHandler))]
		private MoveHandler _moveHandler;

		[SerializeField]
		private PlatformContainer _platformContainer;

		[SerializeField]
		private int _count;

		private Platform[] _platforms;

		private void Awake()
		{
			_platforms = new Platform[_count];
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _moveHandler.CMove(controller);
			Ready(controller.character);
			yield return _ready.CRun(controller);
			Purifiy(controller.character);
			yield return _attack.CRun(controller);
			base.result = Result.Success;
		}

		public void Purifiy(Character owner)
		{
			(from platform in _platforms.ToList()
				where platform != null
				select platform).ToList().ForEach(delegate(Platform platform)
			{
				platform.Purifiy(owner);
			});
		}

		private void Ready(Character owner)
		{
			_platformContainer.NoTentacleTakeTo(_platforms);
			_platforms.Where((Platform platform) => platform != null).ToList().ForEach(delegate(Platform platform)
			{
				platform.ShowSign(owner);
			});
		}
	}
}
