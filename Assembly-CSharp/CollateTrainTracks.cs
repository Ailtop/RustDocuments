using UnityEngine;

public class CollateTrainTracks : ProceduralComponent
{
	private const float MAX_NODE_DIST = 0.1f;

	private const float MAX_NODE_DIST_SQR = 0.0100000007f;

	private const float MAX_NODE_ANGLE = 10f;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		TrainTrackSpline[] array = Object.FindObjectsOfType<TrainTrackSpline>();
		TrainTrackSpline[] array2 = array;
		foreach (TrainTrackSpline ourSpline in array2)
		{
			Vector3 ourStartPos = ourSpline.GetStartPointWorld();
			Vector3 ourEndPos = ourSpline.GetEndPointWorld();
			Vector3 ourStartTangent = ourSpline.GetStartTangentWorld();
			Vector3 ourEndTangent = ourSpline.GetEndTangentWorld();
			if (NodesConnect(ourStartPos, ourEndPos, ourStartTangent, ourEndTangent))
			{
				ourSpline.AddTrackConnection(ourSpline, TrainTrackSpline.TrackPosition.Next, TrainTrackSpline.TrackOrientation.Same);
				ourSpline.AddTrackConnection(ourSpline, TrainTrackSpline.TrackPosition.Prev, TrainTrackSpline.TrackOrientation.Same);
				continue;
			}
			TrainTrackSpline[] array3 = array;
			foreach (TrainTrackSpline otherSpline in array3)
			{
				Vector3 theirStartPos;
				Vector3 theirEndPos;
				Vector3 theirStartTangent;
				Vector3 theirEndTangent;
				if (!(ourSpline == otherSpline))
				{
					theirStartPos = otherSpline.GetStartPointWorld();
					theirEndPos = otherSpline.GetEndPointWorld();
					theirStartTangent = otherSpline.GetStartTangentWorld();
					theirEndTangent = otherSpline.GetEndTangentWorld();
					if (!CompareNodes(ourStart: false, theirStart: true) && !CompareNodes(ourStart: false, theirStart: false) && !CompareNodes(ourStart: true, theirStart: true))
					{
						CompareNodes(ourStart: true, theirStart: false);
					}
				}
				bool CompareNodes(bool ourStart, bool theirStart)
				{
					Vector3 ourPos2 = (ourStart ? ourStartPos : ourEndPos);
					Vector3 ourTangent2 = (ourStart ? ourStartTangent : ourEndTangent);
					Vector3 theirPos2 = (theirStart ? theirStartPos : theirEndPos);
					Vector3 theirTangent2 = (theirStart ? theirStartTangent : theirEndTangent);
					if (ourStart == theirStart)
					{
						theirTangent2 *= -1f;
					}
					if (NodesConnect(ourPos2, theirPos2, ourTangent2, theirTangent2))
					{
						if (ourStart)
						{
							ourSpline.AddTrackConnection(otherSpline, TrainTrackSpline.TrackPosition.Prev, theirStart ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						else
						{
							ourSpline.AddTrackConnection(otherSpline, TrainTrackSpline.TrackPosition.Next, (!theirStart) ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						if (theirStart)
						{
							otherSpline.AddTrackConnection(ourSpline, TrainTrackSpline.TrackPosition.Prev, ourStart ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						else
						{
							otherSpline.AddTrackConnection(ourSpline, TrainTrackSpline.TrackPosition.Next, (!ourStart) ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						return true;
					}
					return false;
				}
			}
		}
		static bool NodesConnect(Vector3 ourPos, Vector3 theirPos, Vector3 ourTangent, Vector3 theirTangent)
		{
			if (Vector3.SqrMagnitude(ourPos - theirPos) < 0.0100000007f)
			{
				return Vector3.Angle(ourTangent, theirTangent) < 10f;
			}
			return false;
		}
	}
}
