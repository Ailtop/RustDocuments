using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class BrothersAI : AIController
	{
		[SerializeField]
		private float _lifeTime = 30f;

		[SerializeField]
		private bool _idleOnStart;

		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _intro;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _outro;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private Attack _attack;

		[SerializeField]
		[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
		private Idle _idle;

		private bool _readyForOutro;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _attack };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			StartCoroutine(COutro());
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			yield return _intro.CRun(this);
			if (_idleOnStart)
			{
				yield return _idle.CRun(this);
			}
			while (!base.dead)
			{
				if (_readyForOutro)
				{
					yield return _outro.CRun(this);
					Object.Destroy(character.gameObject);
				}
				if (base.target == null)
				{
					yield return null;
				}
				else if (base.stuned)
				{
					yield return null;
				}
				else
				{
					yield return _attack.CRun(this);
				}
			}
		}

		private IEnumerator COutro()
		{
			yield return character.chronometer.master.WaitForSeconds(_lifeTime);
			_readyForOutro = true;
		}
	}
}
