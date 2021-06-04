namespace CompanionServer
{
	public interface ITokenBucketSettings
	{
		double MaxTokens { get; }

		double TokensPerSec { get; }
	}
}
