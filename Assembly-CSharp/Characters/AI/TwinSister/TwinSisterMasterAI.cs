using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.TwinSister
{
	public class TwinSisterMasterAI : MonoBehaviour
	{
		private enum DualPattern
		{
			TwinMeteor,
			TwinMeteorGround,
			TwinMeteorChain,
			TwinHomingPierce
		}

		private enum SinglePattern
		{
			MeteorAir,
			DimensionPierce,
			Rush,
			RisingPierce,
			Backstep,
			MeteorGround,
			HomingPierce,
			Dash,
			Idle,
			SkippableIdle
		}

		[SerializeField]
		private Character _master;

		[Header("Intro")]
		[SerializeField]
		private Action _intro;

		[Header("InGame")]
		[Space]
		[SerializeField]
		private Action _attackSuccess;

		[SerializeField]
		private Action _hit;

		[SerializeField]
		private Action _surprise;

		[SerializeField]
		private Action _surpriseFreeze;

		[SerializeField]
		private GoldenAideAI _longHairAide;

		[SerializeField]
		private GoldenAideAI _shortHairAide;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _delayBetweenPattern;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _dualCombatCount;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2Int _twinMeteorChainCount;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _twinMeteorChainTerm;

		[Header("Dual Pattern Weight")]
		[Space]
		[SerializeField]
		[Range(0f, 10f)]
		private int _twinMeteor;

		[SerializeField]
		[Range(0f, 10f)]
		private int _twinMeteorChain;

		[SerializeField]
		[Range(0f, 10f)]
		private int _twinMeteorGround;

		[SerializeField]
		[Range(0f, 10f)]
		private int _twinHomingPierce;

		[Header("Single Pattern Weight")]
		[Space]
		[SerializeField]
		[Range(1f, 10f)]
		private int _resetPoint = 4;

		[Space]
		[Header("Melee")]
		[SerializeField]
		[Range(0f, 10f)]
		private int _meteorAirWeightMelee;

		[SerializeField]
		[Range(0f, 10f)]
		private int _dimensionPierceWeightMelee;

		[SerializeField]
		[Range(0f, 10f)]
		private int _rushWeightMelee = 3;

		[SerializeField]
		[Range(0f, 10f)]
		private int _risingPierceWeightMelee;

		[SerializeField]
		[Range(0f, 10f)]
		private int _backStepWeightMelee = 3;

		[SerializeField]
		[Range(0f, 10f)]
		private int _meteorGroundWeightMelee = 4;

		[Space]
		[Header("Range")]
		[SerializeField]
		[Range(0f, 10f)]
		private int _meteorAirWeightRange = 5;

		[SerializeField]
		[Range(0f, 10f)]
		private int _dimensionPierceWeightRange;

		[SerializeField]
		[Range(0f, 10f)]
		private int _dashWeightRange = 5;

		private GoldenAideAI _fieldAide;

		private GoldenAideAI _behindAide;

		private bool _aliveAlone;

		public bool lockForAwakening;

		private bool _lockForEasterEgg;

		private CoroutineReference _dualPhase;

		[Header("Outro")]
		[SerializeField]
		[Space]
		private Action _outro;

		private List<DualPattern> _dualPatterns;

		private List<SinglePattern> _meleePatterns;

		private List<SinglePattern> _rangePatterns;

		public int goldenAideDiedCount { get; private set; }

		public bool singlePattern { get; set; }

		public void RemovePlayerHitReaction()
		{
			Singleton<Service>.Instance.levelManager.player.health.onTookDamage -= PlayPlayerHitReaction;
		}

		private void PlayPlayerHitReaction([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_attackSuccess.TryStart();
		}

		private void PlayAideHitReaction([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_hit.TryStart();
		}

		public IEnumerator CPlaySurpriseReaction()
		{
			yield return Chronometer.global.WaitForSeconds(4f);
			_surprise.TryStart();
		}

		public void PlayAwakenDieReaction()
		{
			_surpriseFreeze.TryStart();
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				RemovePlayerHitReaction();
			}
		}

		private void Start()
		{
			_longHairAide.character.health.onDiedTryCatch += delegate
			{
				goldenAideDiedCount++;
			};
			_shortHairAide.character.health.onDiedTryCatch += delegate
			{
				goldenAideDiedCount++;
			};
			Singleton<Service>.Instance.levelManager.player.health.onTookDamage += PlayPlayerHitReaction;
			_longHairAide.character.health.onTookDamage += PlayAideHitReaction;
			_shortHairAide.character.health.onTookDamage += PlayAideHitReaction;
			_dualPatterns = new List<DualPattern>(_twinMeteor + _twinMeteorChain + _twinMeteorGround + _twinHomingPierce);
			for (int i = 0; i < _twinMeteor; i++)
			{
				_dualPatterns.Add(DualPattern.TwinMeteor);
			}
			for (int j = 0; j < _twinMeteorChain; j++)
			{
				_dualPatterns.Add(DualPattern.TwinMeteorGround);
			}
			for (int k = 0; k < _twinMeteorGround; k++)
			{
				_dualPatterns.Add(DualPattern.TwinMeteorChain);
			}
			for (int l = 0; l < _twinHomingPierce; l++)
			{
				_dualPatterns.Add(DualPattern.TwinHomingPierce);
			}
			_meleePatterns = new List<SinglePattern>(_meteorAirWeightMelee + _dimensionPierceWeightMelee + _rushWeightMelee + _risingPierceWeightMelee + _backStepWeightMelee);
			for (int m = 0; m < _meteorAirWeightMelee; m++)
			{
				_meleePatterns.Add(SinglePattern.MeteorAir);
			}
			for (int n = 0; n < _dimensionPierceWeightMelee; n++)
			{
				_meleePatterns.Add(SinglePattern.DimensionPierce);
			}
			for (int num = 0; num < _rushWeightMelee; num++)
			{
				_meleePatterns.Add(SinglePattern.Rush);
			}
			for (int num2 = 0; num2 < _risingPierceWeightMelee; num2++)
			{
				_meleePatterns.Add(SinglePattern.RisingPierce);
			}
			for (int num3 = 0; num3 < _backStepWeightMelee; num3++)
			{
				_meleePatterns.Add(SinglePattern.Backstep);
			}
			for (int num4 = 0; num4 < _meteorGroundWeightMelee; num4++)
			{
				_meleePatterns.Add(SinglePattern.MeteorGround);
			}
			_rangePatterns = new List<SinglePattern>(_meteorAirWeightRange + _dimensionPierceWeightRange + _dashWeightRange);
			for (int num5 = 0; num5 < _meteorAirWeightRange; num5++)
			{
				_rangePatterns.Add(SinglePattern.MeteorAir);
			}
			for (int num6 = 0; num6 < _dimensionPierceWeightRange; num6++)
			{
				_rangePatterns.Add(SinglePattern.DimensionPierce);
			}
			for (int num7 = 0; num7 < _dashWeightRange; num7++)
			{
				_rangePatterns.Add(SinglePattern.Dash);
			}
		}

		public IEnumerator CIntro()
		{
			_master.gameObject.SetActive(true);
			_intro.TryStart();
			Vector3 source = Vector3.one * 0.6f;
			Vector3 dest = Vector3.one;
			float duration = 2.63999987f;
			for (float elapsed = 0f; elapsed < duration; elapsed += _master.chronometer.master.deltaTime)
			{
				yield return null;
				_master.transform.localScale = Vector3.Lerp(source, dest, elapsed / duration);
			}
			_master.transform.localScale = dest;
		}

		public IEnumerator RunIntroOut()
		{
			yield return OrderSisterToEscape();
		}

		public IEnumerator ProcessDualCombat()
		{
			float count = Random.Range(_dualCombatCount.x, _dualCombatCount.y);
			DualPattern before = _dualPatterns[_dualPatterns.RandomIndex()];
			for (int i = 0; (float)i < count; i++)
			{
				DualPattern pattern;
				do
				{
					pattern = _dualPatterns[_dualPatterns.RandomIndex()];
				}
				while (before == pattern);
				before = pattern;
				float seconds = Random.Range(_delayBetweenPattern.x, _delayBetweenPattern.y);
				yield return Chronometer.global.WaitForSeconds(seconds);
				if (goldenAideDiedCount > 0)
				{
					break;
				}
				lockForAwakening = true;
				yield return RunDualPattern(pattern);
				lockForAwakening = false;
				if (goldenAideDiedCount > 0)
				{
					break;
				}
			}
		}

		public IEnumerator ProcessSingleCombat(GoldenAideAI fieldAide, GoldenAideAI behindAide)
		{
			_fieldAide = fieldAide;
			_behindAide = behindAide;
			if (fieldAide.dead || behindAide.dead)
			{
				yield break;
			}
			behindAide.character.invulnerable.Attach(this);
			CoroutineReference predelayCoroutine = this.StartCoroutineWithReference(_fieldAide.CStartSinglePhasePreDelay());
			yield return OrderToGoldmaneMeteor();
			while (singlePattern)
			{
				if (fieldAide.dead || behindAide.dead)
				{
					yield break;
				}
				if (fieldAide.CanUseRisingPierce())
				{
					yield return RunSinglePattern(SinglePattern.RisingPierce);
					if (!singlePattern)
					{
						break;
					}
					yield return RunSinglePattern(SinglePattern.Idle);
					continue;
				}
				if (fieldAide.CanUseDimensionPierce())
				{
					yield return RunSinglePattern(SinglePattern.DimensionPierce);
					if (!singlePattern)
					{
						break;
					}
					yield return RunSinglePattern(SinglePattern.SkippableIdle);
					continue;
				}
				SinglePattern pattern = ((!IsMeleeCombat()) ? _rangePatterns.Random() : _meleePatterns.Random());
				yield return RunSinglePattern(pattern);
				if (!singlePattern)
				{
					break;
				}
				switch (pattern)
				{
				case SinglePattern.Dash:
					pattern = ((!MMMaths.Chance(0.3)) ? SinglePattern.MeteorGround : SinglePattern.Rush);
					break;
				case SinglePattern.Backstep:
					pattern = ((!MMMaths.Chance(0.6)) ? SinglePattern.SkippableIdle : SinglePattern.HomingPierce);
					break;
				case SinglePattern.MeteorAir:
				case SinglePattern.MeteorGround:
				case SinglePattern.HomingPierce:
					pattern = SinglePattern.SkippableIdle;
					break;
				case SinglePattern.Rush:
					pattern = SinglePattern.Idle;
					break;
				}
				yield return RunSinglePattern(pattern);
				if (!singlePattern)
				{
					break;
				}
				switch (pattern)
				{
				case SinglePattern.MeteorAir:
				case SinglePattern.MeteorGround:
				case SinglePattern.HomingPierce:
					yield return RunSinglePattern(SinglePattern.SkippableIdle);
					break;
				case SinglePattern.Rush:
					yield return RunSinglePattern(SinglePattern.Idle);
					break;
				}
				if (!singlePattern)
				{
					break;
				}
			}
			behindAide.character.invulnerable.Detach(this);
			predelayCoroutine.Stop();
			yield return OrderToEscape(_fieldAide);
		}

		private bool IsMeleeCombat()
		{
			return _fieldAide.IsMeleeCombat();
		}

		private IEnumerator RunSinglePattern(SinglePattern pattern)
		{
			switch (pattern)
			{
			case SinglePattern.MeteorAir:
				yield return OrderToMeteorInAir();
				break;
			case SinglePattern.DimensionPierce:
				yield return OrderToDimensionPierce();
				break;
			case SinglePattern.Rush:
				yield return OrderToRush();
				break;
			case SinglePattern.Backstep:
				yield return OrderBackStep();
				break;
			case SinglePattern.RisingPierce:
				yield return OrderToRisingPierce();
				break;
			case SinglePattern.MeteorGround:
				yield return OrderToMeteorInGround();
				break;
			case SinglePattern.HomingPierce:
				yield return OrderToHoming();
				break;
			case SinglePattern.Dash:
				yield return OrderToDash();
				break;
			case SinglePattern.Idle:
				yield return OrderToIdle();
				break;
			case SinglePattern.SkippableIdle:
				yield return OrderToSkippableIdle();
				break;
			}
		}

		private IEnumerator RunDualPattern(DualPattern pattern)
		{
			switch (pattern)
			{
			case DualPattern.TwinMeteor:
				yield return OrderTwinMeteor();
				yield return OrderSisterToEscape();
				break;
			case DualPattern.TwinMeteorGround:
				yield return OrderTwinMeteorGround();
				break;
			case DualPattern.TwinMeteorChain:
				yield return OrderTwinMeteorChain();
				break;
			case DualPattern.TwinHomingPierce:
				yield return OrderTwinHomingPierce();
				break;
			}
		}

		private List<SinglePattern> GetSinglePatterns(List<SinglePattern> singlePatterns)
		{
			List<SinglePattern> list = new List<SinglePattern>(_resetPoint);
			for (int i = 0; i < _resetPoint; i++)
			{
				int index = singlePatterns.RandomIndex();
				list.Add(singlePatterns[index]);
				singlePatterns.Remove(singlePatterns[index]);
			}
			singlePatterns.AddRange(list);
			return list;
		}

		private IEnumerator OrderBackStep()
		{
			yield return _fieldAide.CastBackstep();
		}

		private IEnumerator OrderBackStepAndAttack()
		{
			yield return _fieldAide.CastBackstep();
		}

		private IEnumerator OrderTwinMeteor()
		{
			bool num = MMMaths.RandomBool();
			bool flag = MMMaths.RandomBool();
			if (num)
			{
				yield return DoDualBehaviour(_longHairAide.CastTwinMeteor(flag), _shortHairAide.CastPredictTwinMeteor(!flag));
			}
			else
			{
				yield return DoDualBehaviour(_longHairAide.CastPredictTwinMeteor(flag), _shortHairAide.CastTwinMeteor(!flag));
			}
		}

		private IEnumerator OrderTwinMeteorGround()
		{
			bool flag = MMMaths.RandomBool();
			yield return DoDualBehaviour(_longHairAide.CastTwinMeteorGround(flag), _shortHairAide.CastTwinMeteorGround(!flag));
		}

		private IEnumerator OrderTwinMeteorChain()
		{
			int count = Random.Range(_twinMeteorChainCount.x, _twinMeteorChainCount.y);
			for (int i = 0; i < count; i++)
			{
				bool num = MMMaths.RandomBool();
				bool flag = MMMaths.RandomBool();
				float term = Random.Range(_twinMeteorChainTerm.x, _twinMeteorChainTerm.y);
				if (num)
				{
					yield return DoDualBehaviour(_longHairAide.CastTwinMeteorChain(flag, MMMaths.RandomBool()), _shortHairAide.CastTwinMeteorChain(!flag, MMMaths.RandomBool()), term);
				}
				else
				{
					yield return DoDualBehaviour(_shortHairAide.CastTwinMeteorChain(flag, MMMaths.RandomBool()), _longHairAide.CastTwinMeteorChain(!flag, MMMaths.RandomBool()), term);
				}
				if (goldenAideDiedCount > 0 || singlePattern)
				{
					break;
				}
			}
		}

		private IEnumerator OrderTwinHomingPierce()
		{
			bool flag = MMMaths.RandomBool();
			yield return DoDualBehaviour(_longHairAide.CastTwinMeteorPierce(flag), _shortHairAide.CastTwinMeteorPierce(!flag));
		}

		private IEnumerator DoDualBehaviour(IEnumerator behaviour1, IEnumerator behaviour2, float term = 0f)
		{
			_003C_003Ec__DisplayClass67_0 _003C_003Ec__DisplayClass67_ = new _003C_003Ec__DisplayClass67_0();
			_003C_003Ec__DisplayClass67_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass67_.success = 0;
			StartCoroutine(_003C_003Ec__DisplayClass67_._003CDoDualBehaviour_003Eg__StartBehaviour_007C0(behaviour1));
			if (term > 0f)
			{
				yield return Chronometer.global.WaitForSeconds(term);
			}
			StartCoroutine(_003C_003Ec__DisplayClass67_._003CDoDualBehaviour_003Eg__StartBehaviour_007C0(behaviour2));
			while (_003C_003Ec__DisplayClass67_.success < 2)
			{
				yield return null;
			}
		}

		private IEnumerator OrderToGoldmaneMeteor()
		{
			yield return _fieldAide.CastGoldenMeteor();
		}

		private IEnumerator OrderToMeteorInAir()
		{
			yield return _fieldAide.CastMeteorInAir();
		}

		private IEnumerator OrderToMeteorInGround()
		{
			yield return _fieldAide.CastMeteorInGround();
		}

		private IEnumerator OrderToMeteorInGround2()
		{
			yield return _fieldAide.CastMeteorInGround2();
		}

		private IEnumerator OrderToRush()
		{
			yield return _fieldAide.CastRush();
		}

		private IEnumerator OrderToHoming()
		{
			yield return _fieldAide.CastRangeAttackHoming(false);
		}

		private IEnumerator OrderToDash()
		{
			yield return _fieldAide.CastDash();
		}

		private IEnumerator OrderToDimensionPierce()
		{
			yield return _fieldAide.CastDimensionPierce();
		}

		private IEnumerator OrderToRisingPierce()
		{
			yield return _fieldAide.CastRisingPierce();
		}

		private IEnumerator OrderToIdle()
		{
			yield return _fieldAide.CastIdle();
		}

		private IEnumerator OrderToSkippableIdle()
		{
			yield return _fieldAide.CastSkippableIdle();
		}

		private IEnumerator OrderToEscape(GoldenAideAI goldenAide)
		{
			yield return goldenAide.EscapeForTwinMeteor();
		}

		private IEnumerator OrderSisterToEscape()
		{
			yield return DoDualBehaviour(_longHairAide.EscapeForTwinMeteor(), _shortHairAide.EscapeForTwinMeteor());
		}

		public IEnumerator COutro()
		{
			_outro.TryStart();
			Vector3 source = Vector3.one;
			Vector3 dest = Vector3.one * 0.6f;
			float duration = 2.63999987f;
			for (float elapsed = 0f; elapsed < duration; elapsed += _master.chronometer.master.deltaTime)
			{
				yield return null;
				_master.transform.localScale = Vector3.Lerp(source, dest, elapsed / duration);
			}
			_master.transform.localScale = dest;
			while (_outro.running)
			{
				yield return null;
			}
			_master.gameObject.SetActive(false);
			yield return Chronometer.global.WaitForSeconds(1f);
		}
	}
}
