using System;
using System.Linq;
using FX;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Enemies
{
	[Serializable]
	public class CurseOfLight : Ability
	{
		public class Instance : AbilityInstance<CurseOfLight>
		{
			private static float increasement = 0.1f;

			private static readonly Stat.Values _statPerStack = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, 1f + increasement));

			private readonly EffectInfo _effectInfo;

			private readonly SoundInfo _soundInfo;

			private const int _phase = 3;

			private int _stacks;

			private Stat.Values _stat;

			private string floatingText => Lingua.GetLocalizedString("floating/curseoflight");

			public override int iconStacks => _stacks;

			public override Sprite icon => CurseOfLightResource.instance.icon;

			public Instance(Character owner, CurseOfLight ability)
				: base(owner, ability)
			{
				_effectInfo = new EffectInfo(CurseOfLightResource.instance.effect)
				{
					attachInfo = new EffectInfo.AttachInfo(true, false, 1, EffectInfo.AttachInfo.Pivot.Bottom),
					trackChildren = false,
					sortingLayerId = SortingLayer.layers.Last().id
				};
				_soundInfo = new SoundInfo(CurseOfLightResource.instance.sfx);
			}

			protected override void OnAttach()
			{
				_stat = _statPerStack.Clone();
				_stacks = 1;
				SpawnEffects();
			}

			protected override void OnDetach()
			{
				Scene<GameBase>.instance.uiManager.curseOfLightVignette.UpdateStack(0f);
				owner.stat.DetachValues(_stat);
			}

			public override void Refresh()
			{
				base.Refresh();
				_stacks++;
				if (_stacks == 3)
				{
					AttachStatBonus();
				}
				else if (_stacks % 3 == 0)
				{
					UpdateStack();
				}
				SpawnEffects();
			}

			private void AttachStatBonus()
			{
				Scene<GameBase>.instance.uiManager.curseOfLightVignette.UpdateStack(_stacks);
				owner.stat.AttachValues(_stat);
				SpawnBuffText();
			}

			private void UpdateStack()
			{
				Scene<GameBase>.instance.uiManager.curseOfLightVignette.UpdateStack(_stacks);
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = _stat.values[i].value + (double)increasement;
				}
				owner.stat.SetNeedUpdate();
				SpawnBuffText();
			}

			private void SpawnEffects()
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_soundInfo, owner.transform.position);
				_effectInfo.Spawn(owner.transform.position, owner);
			}

			private void SpawnBuffText()
			{
				float num = (float)(_stacks / 3) * increasement * 100f;
				string text = string.Format(floatingText, num);
				Vector3 center = owner.collider.bounds.center;
				Singleton<Service>.Instance.floatingTextSpawner.SpawnBuff(text, center);
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
