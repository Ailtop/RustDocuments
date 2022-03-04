namespace CompanionServer
{
	public enum NotificationSendResult
	{
		Failed = 0,
		Sent = 1,
		Empty = 2,
		Disabled = 3,
		RateLimited = 4,
		ServerError = 5,
		NoTargetsFound = 6,
		TooManySubscribers = 7
	}
}
