using System.Collections;
using Characters;
using Characters.Abilities;
using Characters.Operations;
using Characters.Operations.Attack;
using Level.Traps;
using UnityEditor;
using UnityEngine;

namespace Level.Pope
{
	public class Fire : Trap
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onAppear;

		[SerializeField]
		[Subcomponent(typeof(SweepAttack))]
		private SweepAttack _attack;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher _abilityAttacher;

		private void Awake()
		{
			_attack.Initialize();
			_attack.Run(_character);
			_onAppear.Initialize();
			_abilityAttacher.Initialize(_character);
			_abilityAttacher.StartAttach();
		}

		public void Appear()
		{
			base.gameObject.SetActive(true);
			StartCoroutine(_onAppear.CRun(_character));
			_attack.Run(_character);
			StartCoroutine(CAppear());
		}

		public void Disappear()
		{
			_attack.Stop();
			StartCoroutine(CDisappear());
		}

		private IEnumerator CAppear()
		{
			int num = 3;
			int time = 6;
			float elapsed = 0f;
			Vector2 start = base.transform.position;
			Vector2 end = new Vector2(start.x, start.y + (float)num);
			while (elapsed < (float)time)
			{
				base.transform.position = Vector2.Lerp(start, end, elapsed / (float)time);
				elapsed += _character.chronometer.master.deltaTime;
				yield return null;
			}
			base.transform.position = end;
		}

		private IEnumerator CDisappear()
		{
			int num = 4;
			int time = 4;
			float elapsed = 0f;
			Vector2 start = base.transform.position;
			Vector2 end = new Vector2(start.x, start.y - (float)num);
			while (elapsed < (float)time)
			{
				base.transform.position = Vector2.Lerp(start, end, elapsed / (float)time);
				elapsed += _character.chronometer.master.deltaTime;
				yield return null;
			}
			base.transform.position = end;
			base.gameObject.SetActive(false);
		}
	}
}
