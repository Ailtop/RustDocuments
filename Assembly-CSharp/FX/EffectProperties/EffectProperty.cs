using Characters;

namespace FX.EffectProperties
{
	public abstract class EffectProperty
	{
		public abstract void Apply(PoolObject spawned, Character owner, Target target);
	}
}
