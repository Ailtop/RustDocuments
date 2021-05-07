using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Hero;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class FirstHeroPhase1Test : AIController
	{
		[Header("Slash")]
		[SerializeField]
		[Subcomponent(typeof(BackSlashA))]
		private BackSlashA _basicSlash;

		[SerializeField]
		[Subcomponent(typeof(BackSlashB))]
		private BackSlashB _horizontalSlash;

		[SerializeField]
		[Subcomponent(typeof(VerticalSlash))]
		private VerticalSlash _verticalSlash;

		[Header("Basic Skill")]
		[SerializeField]
		[Subcomponent(typeof(Landing))]
		private Landing _landing;

		[Header("Dash")]
		[SerializeField]
		[Subcomponent(typeof(Characters.AI.Behaviours.Hero.Dash))]
		private Characters.AI.Behaviours.Hero.Dash _dash;

		[SerializeField]
		[Subcomponent(typeof(DashBreakAway))]
		private DashBreakAway _dashBreakAway;

		[Header("Template")]
		[SerializeField]
		private BehaviourTemplate _behaviourA;

		[SerializeField]
		private BehaviourTemplate _behaviourB;

		[SerializeField]
		private BehaviourTemplate _behaviourC;

		[SerializeField]
		private BehaviourTemplate _behaviourD;

		[SerializeField]
		private BehaviourTemplate _behaviourE;

		[SerializeField]
		private BehaviourTemplate _behaviourF;

		[SerializeField]
		private BehaviourTemplate _behaviourG;

		[SerializeField]
		private BehaviourTemplate _behaviourH;

		[SerializeField]
		private BehaviourTemplate _behaviourI;

		[SerializeField]
		private BehaviourTemplate _behaviourJ;

		[SerializeField]
		private BehaviourTemplate _behaviourK;

		[SerializeField]
		private BehaviourTemplate _behaviourL;

		[SerializeField]
		private BehaviourTemplate _behaviourM;

		[SerializeField]
		private BehaviourTemplate _behaviourN;

		[Header("Idle")]
		[SerializeField]
		[Subcomponent(typeof(SkipableIdle))]
		private SkipableIdle _skipableIdle;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[Header("Behaviour Chance")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourA_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourB_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourC_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourG_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourH_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourK_Chance = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _behaviourN_Chance = 0.5f;

		[Header("Trigger")]
		[SerializeField]
		[Subcomponent(typeof(Trigger))]
		private Trigger _trigger;

		private Characters.AI.Behaviours.Behaviour[] _slash;

		private void Awake()
		{
			_slash = new Characters.AI.Behaviours.Behaviour[3] { _basicSlash, _horizontalSlash, _verticalSlash };
		}

		private new void OnEnable()
		{
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			while (true)
			{
				if (_trigger.InShortRange(this))
				{
					if (_trigger.CanRunDashBreakAway(this))
					{
						yield return _dashBreakAway.CRun(this);
						continue;
					}
					if (_trigger.CanRunBehavourE(this))
					{
						yield return _behaviourE.CRun(this);
						yield return _idle.CRun(this);
						continue;
					}
					switch (Random.Range(0, 3))
					{
					case 0:
						yield return _slash.Random().CRun(this);
						if (MMMaths.Chance(_behaviourA_Chance))
						{
							yield return _behaviourA.CRun(this);
						}
						yield return _skipableIdle.CRun(this);
						break;
					case 1:
						yield return _landing.CRun(this);
						if (MMMaths.Chance(_behaviourB_Chance))
						{
							yield return _behaviourB.CRun(this);
							if (MMMaths.Chance(_behaviourC_Chance))
							{
								yield return _behaviourC.CRun(this);
							}
						}
						yield return _skipableIdle.CRun(this);
						break;
					case 2:
						yield return _behaviourD.CRun(this);
						yield return _skipableIdle.CRun(this);
						break;
					}
					continue;
				}
				if (_trigger.InMiddleRange(this))
				{
					if (_trigger.CanRunBehavourJ(this))
					{
						yield return _behaviourJ.CRun(this);
						if (MMMaths.Chance(_behaviourK_Chance))
						{
							yield return _behaviourK.CRun(this);
						}
						yield return _idle.CRun(this);
						continue;
					}
					switch (Random.Range(0, 3))
					{
					case 0:
						yield return _dash.CRun(this);
						yield return _behaviourF.CRun(this);
						yield return _skipableIdle.CRun(this);
						break;
					case 1:
						yield return _landing.CRun(this);
						if (MMMaths.Chance(_behaviourG_Chance))
						{
							yield return _behaviourG.CRun(this);
							if (MMMaths.Chance(_behaviourH_Chance))
							{
								yield return _behaviourH.CRun(this);
							}
						}
						yield return _skipableIdle.CRun(this);
						break;
					case 2:
						yield return _behaviourJ.CRun(this);
						yield return _skipableIdle.CRun(this);
						break;
					}
					continue;
				}
				if (MMMaths.RandomBool())
				{
					yield return _dash.CRun(this);
					yield return _behaviourL.CRun(this);
				}
				else
				{
					yield return _behaviourM.CRun(this);
					if (MMMaths.Chance(_behaviourN_Chance))
					{
						yield return _behaviourN.CRun(this);
					}
				}
				yield return _skipableIdle.CRun(this);
			}
		}
	}
}
