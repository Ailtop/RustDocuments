namespace CompanionServer.Handlers
{
	public enum ValidationResult
	{
		Success = 0,
		NotFound = 1,
		RateLimit = 2,
		Banned = 3,
		Rejected = 4
	}
}
