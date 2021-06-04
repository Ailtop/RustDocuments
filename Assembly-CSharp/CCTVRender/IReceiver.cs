using System;

namespace CCTVRender
{
	public interface IReceiver
	{
		void RenderCompleted(uint requestId, uint frame, Span<byte> jpgImage);
	}
}
