using System.Collections;
using Characters;
using UnityEditor;
using UnityEngine;

namespace BT
{
	public class BehaviourTreeRunner : MonoBehaviour
	{
		[SerializeField]
		private bool _runOnEnable = true;

		[SerializeField]
		private ContextSetting _setting;

		[SerializeField]
		[Subcomponent(typeof(BehaviourTree))]
		private BehaviourTree _root;

		private Context _context = new Context();

		public Context context => _context;

		private void OnEnable()
		{
			if (_runOnEnable)
			{
				Run();
			}
		}

		public void Run()
		{
			_context = Context.Create();
			_setting.ApplyTo(_context);
			StartCoroutine(CRun());
		}

		public void Run(Context context)
		{
			_context = context;
			StartCoroutine(CRun());
		}

		private IEnumerator CRun()
		{
			Character character = _context.Get<Character>(Key.OwnerCharacter);
			_root.ResetState();
			NodeState result;
			do
			{
				_context.deltaTime = ((character == null) ? Chronometer.global.deltaTime : character.chronometer.master.deltaTime);
				result = _root.Tick(_context);
				yield return null;
			}
			while (result == NodeState.Running);
		}
	}
}
