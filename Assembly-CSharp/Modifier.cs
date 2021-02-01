using Facepunch;
using ProtoBuf;

public class Modifier
{
	public enum ModifierType
	{
		Wood_Yield,
		Ore_Yield,
		Radiation_Resistance,
		Radiation_Exposure_Resistance,
		Max_Health,
		Scrap_Yield
	}

	public enum ModifierSource
	{
		Tea
	}

	public ModifierType Type
	{
		get;
		private set;
	}

	public ModifierSource Source
	{
		get;
		private set;
	}

	public float Value
	{
		get;
		private set;
	} = 1f;


	public float Duration
	{
		get;
		private set;
	} = 10f;


	public float TimeRemaining
	{
		get;
		private set;
	}

	public bool Expired
	{
		get;
		private set;
	}

	public void Init(ModifierType type, ModifierSource source, float value, float duration, float remaining)
	{
		Type = type;
		Source = source;
		Value = value;
		Duration = duration;
		Expired = false;
		TimeRemaining = remaining;
	}

	public void Tick(BaseCombatEntity ownerEntity, float delta)
	{
		TimeRemaining -= delta;
		Expired = TimeRemaining <= 0f;
	}

	public ProtoBuf.Modifier Save()
	{
		ProtoBuf.Modifier modifier = Pool.Get<ProtoBuf.Modifier>();
		modifier.type = (int)Type;
		modifier.source = (int)Source;
		modifier.value = Value;
		modifier.timeRemaing = TimeRemaining;
		return modifier;
	}

	public void Load(ProtoBuf.Modifier m)
	{
		Type = (ModifierType)m.type;
		Source = (ModifierSource)m.source;
		Value = m.value;
		TimeRemaining = m.timeRemaing;
	}
}
