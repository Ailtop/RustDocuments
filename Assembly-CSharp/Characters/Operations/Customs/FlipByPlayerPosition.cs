using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class FlipByPlayerPosition : Operation
	{
		[SerializeField]
		private Transform _body;

		public override void Run()
		{
			Transform transform = Singleton<Service>.Instance.levelManager.player.transform;
			if (_body.position.x < transform.position.x)
			{
				_body.localScale = new Vector2(1f, 1f);
			}
			else
			{
				_body.localScale = new Vector2(-1f, 1f);
			}
		}
	}
}
