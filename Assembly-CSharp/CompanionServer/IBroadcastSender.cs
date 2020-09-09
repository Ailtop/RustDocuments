using System.Collections.Generic;

namespace CompanionServer
{
	public interface IBroadcastSender<TTarget, TMessage> where TTarget : class
	{
		void BroadcastTo(List<TTarget> targets, TMessage message);
	}
}
