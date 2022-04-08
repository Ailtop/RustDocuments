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
		_003C_003Ec__DisplayClass5_0 _003C_003Ec__DisplayClass5_ = default(_003C_003Ec__DisplayClass5_0);
		_003C_003Ec__DisplayClass5_1 _003C_003Ec__DisplayClass5_2 = default(_003C_003Ec__DisplayClass5_1);
		_003C_003Ec__DisplayClass5_2 _003C_003Ec__DisplayClass5_3 = default(_003C_003Ec__DisplayClass5_2);
		_003C_003Ec__DisplayClass5_3 _003C_003Ec__DisplayClass5_4 = default(_003C_003Ec__DisplayClass5_3);
		for (int i = 0; i < array2.Length; i++)
		{
			_003C_003Ec__DisplayClass5_.ourSpline = array2[i];
			_003C_003Ec__DisplayClass5_2.ourStartPos = _003C_003Ec__DisplayClass5_.ourSpline.GetStartPointWorld();
			_003C_003Ec__DisplayClass5_2.ourEndPos = _003C_003Ec__DisplayClass5_.ourSpline.GetEndPointWorld();
			_003C_003Ec__DisplayClass5_2.ourStartTangent = _003C_003Ec__DisplayClass5_.ourSpline.GetStartTangentWorld();
			_003C_003Ec__DisplayClass5_2.ourEndTangent = _003C_003Ec__DisplayClass5_.ourSpline.GetEndTangentWorld();
			if (_003CProcess_003Eg__NodesConnect_007C5_0(_003C_003Ec__DisplayClass5_2.ourStartPos, _003C_003Ec__DisplayClass5_2.ourEndPos, _003C_003Ec__DisplayClass5_2.ourStartTangent, _003C_003Ec__DisplayClass5_2.ourEndTangent))
			{
				_003C_003Ec__DisplayClass5_.ourSpline.AddTrackConnection(_003C_003Ec__DisplayClass5_.ourSpline, TrainTrackSpline.TrackPosition.Next, TrainTrackSpline.TrackOrientation.Same);
				_003C_003Ec__DisplayClass5_.ourSpline.AddTrackConnection(_003C_003Ec__DisplayClass5_.ourSpline, TrainTrackSpline.TrackPosition.Prev, TrainTrackSpline.TrackOrientation.Same);
				continue;
			}
			TrainTrackSpline[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				_003C_003Ec__DisplayClass5_3.otherSpline = array3[j];
				if (!(_003C_003Ec__DisplayClass5_.ourSpline == _003C_003Ec__DisplayClass5_3.otherSpline))
				{
					_003C_003Ec__DisplayClass5_4.theirStartPos = _003C_003Ec__DisplayClass5_3.otherSpline.GetStartPointWorld();
					_003C_003Ec__DisplayClass5_4.theirEndPos = _003C_003Ec__DisplayClass5_3.otherSpline.GetEndPointWorld();
					_003C_003Ec__DisplayClass5_4.theirStartTangent = _003C_003Ec__DisplayClass5_3.otherSpline.GetStartTangentWorld();
					_003C_003Ec__DisplayClass5_4.theirEndTangent = _003C_003Ec__DisplayClass5_3.otherSpline.GetEndTangentWorld();
					if (!_003CProcess_003Eg__CompareNodes_007C5_1(false, true, ref _003C_003Ec__DisplayClass5_, ref _003C_003Ec__DisplayClass5_2, ref _003C_003Ec__DisplayClass5_3, ref _003C_003Ec__DisplayClass5_4) && !_003CProcess_003Eg__CompareNodes_007C5_1(false, false, ref _003C_003Ec__DisplayClass5_, ref _003C_003Ec__DisplayClass5_2, ref _003C_003Ec__DisplayClass5_3, ref _003C_003Ec__DisplayClass5_4) && !_003CProcess_003Eg__CompareNodes_007C5_1(true, true, ref _003C_003Ec__DisplayClass5_, ref _003C_003Ec__DisplayClass5_2, ref _003C_003Ec__DisplayClass5_3, ref _003C_003Ec__DisplayClass5_4))
					{
						_003CProcess_003Eg__CompareNodes_007C5_1(true, false, ref _003C_003Ec__DisplayClass5_, ref _003C_003Ec__DisplayClass5_2, ref _003C_003Ec__DisplayClass5_3, ref _003C_003Ec__DisplayClass5_4);
					}
				}
			}
		}
	}
}
