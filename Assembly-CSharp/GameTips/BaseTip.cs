namespace GameTips;

public abstract class BaseTip
{
	public abstract bool ShouldShow { get; }

	public string Type => GetType().Name;

	public abstract Translate.Phrase GetPhrase();
}
