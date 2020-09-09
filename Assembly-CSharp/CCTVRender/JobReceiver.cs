using System;

namespace CCTVRender
{
	public struct JobReceiver
	{
		public uint RequestId
		{
			get;
		}

		public IReceiver Receiver
		{
			get;
		}

		public JobReceiver(uint requestId, IReceiver receiver)
		{
			RequestId = requestId;
			if (receiver == null)
			{
				throw new ArgumentNullException("receiver");
			}
			Receiver = receiver;
		}
	}
}
