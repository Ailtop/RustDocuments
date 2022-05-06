using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class TrainTrackSpline : WorldSpline
{
	public enum TrackSelection
	{
		Default = 0,
		Left = 1,
		Right = 2
	}

	public enum TrackPosition
	{
		Next = 0,
		Prev = 1
	}

	public enum TrackOrientation
	{
		Same = 0,
		Reverse = 1
	}

	public class ConnectedTrackInfo
	{
		public TrainTrackSpline track;

		public TrackOrientation orientation;

		public float angle;

		public ConnectedTrackInfo(TrainTrackSpline track, TrackOrientation orientation, float angle)
		{
			this.track = track;
			this.orientation = orientation;
			this.angle = angle;
		}
	}

	public enum DistanceType
	{
		SplineDistance = 0,
		WorldDistance = 1
	}

	public interface ITrainTrackUser
	{
		Vector3 Position { get; }

		float FrontWheelSplineDist { get; }

		Vector3 GetWorldVelocity();
	}

	[Tooltip("Is this track spline part of a train station?")]
	public bool isStation;

	[Tooltip("Can above-ground trains spawn here?")]
	public bool aboveGroundSpawn;

	public bool useNewTangentCalc;

	public bool forceAsSecondary;

	public static List<TrainTrackSpline> SidingSplines = new List<TrainTrackSpline>();

	public List<ConnectedTrackInfo> nextTracks = new List<ConnectedTrackInfo>();

	public int straightestNextIndex;

	public List<ConnectedTrackInfo> prevTracks = new List<ConnectedTrackInfo>();

	public int straightestPrevIndex;

	public HashSet<ITrainTrackUser> trackUsers = new HashSet<ITrainTrackUser>();

	public bool HasNextTrack => nextTracks.Count > 0;

	public bool HasPrevTrack => prevTracks.Count > 0;

	public void SetAll(Vector3[] points, Vector3[] tangents, TrainTrackSpline sourceSpline)
	{
		base.points = points;
		base.tangents = tangents;
		lutInterval = sourceSpline.lutInterval;
		isStation = sourceSpline.isStation;
		aboveGroundSpawn = sourceSpline.aboveGroundSpawn;
		useNewTangentCalc = sourceSpline.useNewTangentCalc;
	}

	public float GetSplineDistAfterMove(float prevSplineDist, Vector3 askerForward, float distMoved, TrackSelection trackSelection, out TrainTrackSpline onSpline, out bool atEndOfLine, TrainTrackSpline preferredAltTrack = null)
	{
		bool facingForward = IsForward(askerForward, prevSplineDist);
		return GetSplineDistAfterMove(prevSplineDist, distMoved, trackSelection, facingForward, out onSpline, out atEndOfLine, preferredAltTrack);
	}

	public float GetSplineDistAfterMove(float prevSplineDist, float distMoved, TrackSelection trackSelection, bool facingForward, out TrainTrackSpline onSpline, out bool atEndOfLine, TrainTrackSpline preferredAltTrack = null)
	{
		WorldSplineData data = GetData();
		float num = (facingForward ? (prevSplineDist + distMoved) : (prevSplineDist - distMoved));
		atEndOfLine = false;
		onSpline = this;
		if (num < 0f)
		{
			if (HasPrevTrack)
			{
				ConnectedTrackInfo trackSelection2 = GetTrackSelection(prevTracks, straightestPrevIndex, trackSelection, nextTrack: false, facingForward, preferredAltTrack);
				float distMoved2 = (facingForward ? num : (0f - num));
				if (trackSelection2.orientation == TrackOrientation.Same)
				{
					prevSplineDist = trackSelection2.track.GetLength();
				}
				else
				{
					prevSplineDist = 0f;
					facingForward = !facingForward;
				}
				return trackSelection2.track.GetSplineDistAfterMove(prevSplineDist, distMoved2, trackSelection, facingForward, out onSpline, out atEndOfLine);
			}
			atEndOfLine = true;
			num = 0f;
		}
		else if (num > data.Length)
		{
			if (HasNextTrack)
			{
				ConnectedTrackInfo trackSelection3 = GetTrackSelection(nextTracks, straightestNextIndex, trackSelection, nextTrack: true, facingForward, preferredAltTrack);
				float distMoved3 = (facingForward ? (num - data.Length) : (0f - (num - data.Length)));
				if (trackSelection3.orientation == TrackOrientation.Same)
				{
					prevSplineDist = 0f;
				}
				else
				{
					prevSplineDist = trackSelection3.track.GetLength();
					facingForward = !facingForward;
				}
				return trackSelection3.track.GetSplineDistAfterMove(prevSplineDist, distMoved3, trackSelection, facingForward, out onSpline, out atEndOfLine);
			}
			atEndOfLine = true;
			num = data.Length;
		}
		return num;
	}

	public float GetDistance(Vector3 position, float maxError, out float minSplineDist)
	{
		WorldSplineData data = GetData();
		float num = maxError * maxError;
		Vector3 vector = base.transform.InverseTransformPoint(position);
		float num2 = float.MaxValue;
		minSplineDist = 0f;
		int num3 = 0;
		int num4 = data.LUTValues.Count;
		if (data.Length > 40f)
		{
			for (int i = 0; (float)i < data.Length + 10f; i += 10)
			{
				float num5 = Vector3.SqrMagnitude(data.GetPointCubicHermite(i) - vector);
				if (num5 < num2)
				{
					num2 = num5;
					minSplineDist = i;
				}
			}
			num3 = Mathf.FloorToInt(Mathf.Max(0f, minSplineDist - 10f + 1f));
			num4 = Mathf.CeilToInt(Mathf.Min(data.LUTValues.Count, minSplineDist + 10f - 1f));
		}
		for (int j = num3; j < num4; j++)
		{
			WorldSplineData.LUTEntry lUTEntry = data.LUTValues[j];
			for (int k = 0; k < lUTEntry.points.Count; k++)
			{
				WorldSplineData.LUTEntry.LUTPoint lUTPoint = lUTEntry.points[k];
				float num6 = Vector3.SqrMagnitude(lUTPoint.pos - vector);
				if (num6 < num2)
				{
					num2 = num6;
					minSplineDist = lUTPoint.distance;
					if (num6 < num)
					{
						break;
					}
				}
			}
		}
		return Mathf.Sqrt(num2);
	}

	public float GetLength()
	{
		return GetData().Length;
	}

	public Vector3 GetPosition(float distance)
	{
		return GetPointCubicHermiteWorld(distance);
	}

	public Vector3 GetPositionAndTangent(float distance, Vector3 askerForward, out Vector3 tangent)
	{
		Vector3 pointAndTangentCubicHermiteWorld = GetPointAndTangentCubicHermiteWorld(distance, out tangent);
		if (Vector3.Dot(askerForward, tangent) < 0f)
		{
			tangent = -tangent;
		}
		return pointAndTangentCubicHermiteWorld;
	}

	public void AddTrackConnection(TrainTrackSpline track, TrackPosition p, TrackOrientation o)
	{
		List<ConnectedTrackInfo> list = ((p == TrackPosition.Next) ? nextTracks : prevTracks);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].track == track)
			{
				return;
			}
		}
		Vector3 position = ((p == TrackPosition.Next) ? points[points.Length - 2] : points[0]);
		Vector3 position2 = ((p == TrackPosition.Next) ? points[points.Length - 1] : points[1]);
		Vector3 from = base.transform.TransformPoint(position2) - base.transform.TransformPoint(position);
		Vector3 initialVector = GetInitialVector(track, p, o);
		float num = Vector3.SignedAngle(from, initialVector, Vector3.up);
		int j;
		for (j = 0; j < list.Count && !(list[j].angle > num); j++)
		{
		}
		list.Insert(j, new ConnectedTrackInfo(track, o, num));
		float num2 = float.MaxValue;
		int num3 = 0;
		for (int k = 0; k < list.Count; k++)
		{
			ConnectedTrackInfo connectedTrackInfo = list[k];
			if (connectedTrackInfo.track.forceAsSecondary)
			{
				continue;
			}
			float num4 = Mathf.Abs(connectedTrackInfo.angle);
			if (num4 < num2)
			{
				num2 = num4;
				num3 = k;
				if (num2 == 0f)
				{
					break;
				}
			}
		}
		if (p == TrackPosition.Next)
		{
			straightestNextIndex = num3;
		}
		else
		{
			straightestPrevIndex = num3;
		}
	}

	public void RegisterTrackUser(ITrainTrackUser user)
	{
		trackUsers.Add(user);
	}

	public void DeregisterTrackUser(ITrainTrackUser user)
	{
		if (user != null)
		{
			trackUsers.Remove(user);
		}
	}

	public bool IsForward(Vector3 askerForward, float askerSplineDist)
	{
		WorldSplineData data = GetData();
		Vector3 rhs = ((!useNewTangentCalc) ? GetTangentWorld(askerSplineDist, data) : GetTangentCubicHermiteWorld(askerSplineDist, data));
		return Vector3.Dot(askerForward, rhs) >= 0f;
	}

	public bool HasValidHazardWithin(TrainCar asker, float askerSplineDist, float minHazardDist, float maxHazardDist, TrackSelection trackSelection, float trackSpeed, TrainTrackSpline preferredAltTrack = null)
	{
		Vector3 askerForward = ((trackSpeed >= 0f) ? asker.transform.forward : (-asker.transform.forward));
		bool movingForward = IsForward(askerForward, askerSplineDist);
		return HasValidHazardWithin(asker, askerForward, askerSplineDist, minHazardDist, maxHazardDist, trackSelection, movingForward, preferredAltTrack);
	}

	public bool HasValidHazardWithin(ITrainTrackUser asker, Vector3 askerForward, float askerSplineDist, float minHazardDist, float maxHazardDist, TrackSelection trackSelection, bool movingForward, TrainTrackSpline preferredAltTrack = null)
	{
		WorldSplineData data = GetData();
		foreach (ITrainTrackUser trackUser in trackUsers)
		{
			if (trackUser == asker)
			{
				continue;
			}
			Vector3 rhs = trackUser.Position - asker.Position;
			if (!(Vector3.Dot(askerForward, rhs) >= 0f))
			{
				continue;
			}
			float magnitude = rhs.magnitude;
			if (magnitude > minHazardDist && magnitude < maxHazardDist)
			{
				Vector3 worldVelocity = trackUser.GetWorldVelocity();
				if (worldVelocity.sqrMagnitude < 4f || Vector3.Dot(worldVelocity, rhs) < 0f)
				{
					return true;
				}
			}
		}
		float num = (movingForward ? (askerSplineDist + minHazardDist) : (askerSplineDist - minHazardDist));
		float num2 = (movingForward ? (askerSplineDist + maxHazardDist) : (askerSplineDist - maxHazardDist));
		if (num2 < 0f)
		{
			if (HasPrevTrack)
			{
				ConnectedTrackInfo trackSelection2 = GetTrackSelection(prevTracks, straightestPrevIndex, trackSelection, nextTrack: false, movingForward, preferredAltTrack);
				if (trackSelection2.orientation == TrackOrientation.Same)
				{
					askerSplineDist = trackSelection2.track.GetLength();
				}
				else
				{
					askerSplineDist = 0f;
					movingForward = !movingForward;
				}
				float minHazardDist2 = Mathf.Max(0f - num, 0f);
				float maxHazardDist2 = 0f - num2;
				return trackSelection2.track.HasValidHazardWithin(asker, askerForward, askerSplineDist, minHazardDist2, maxHazardDist2, trackSelection, movingForward, preferredAltTrack);
			}
		}
		else if (num2 > data.Length && HasNextTrack)
		{
			ConnectedTrackInfo trackSelection3 = GetTrackSelection(nextTracks, straightestNextIndex, trackSelection, nextTrack: true, movingForward, preferredAltTrack);
			if (trackSelection3.orientation == TrackOrientation.Same)
			{
				askerSplineDist = 0f;
			}
			else
			{
				askerSplineDist = trackSelection3.track.GetLength();
				movingForward = !movingForward;
			}
			float minHazardDist3 = Mathf.Max(num - data.Length, 0f);
			float maxHazardDist3 = num2 - data.Length;
			return trackSelection3.track.HasValidHazardWithin(asker, askerForward, askerSplineDist, minHazardDist3, maxHazardDist3, trackSelection, movingForward, preferredAltTrack);
		}
		return false;
	}

	public bool HasClearTrackSpaceNear(ITrainTrackUser asker)
	{
		if (!HasClearTrackSpace(asker))
		{
			return false;
		}
		if (HasNextTrack)
		{
			foreach (ConnectedTrackInfo nextTrack in nextTracks)
			{
				if (!nextTrack.track.HasClearTrackSpace(asker))
				{
					return false;
				}
			}
		}
		if (HasPrevTrack)
		{
			foreach (ConnectedTrackInfo prevTrack in prevTracks)
			{
				if (!prevTrack.track.HasClearTrackSpace(asker))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool HasClearTrackSpace(ITrainTrackUser asker)
	{
		foreach (ITrainTrackUser trackUser in trackUsers)
		{
			if (trackUser != asker && Vector3.SqrMagnitude(trackUser.Position - asker.Position) < 144f)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasConnectedTrack(TrainTrackSpline tts)
	{
		foreach (ConnectedTrackInfo nextTrack in nextTracks)
		{
			if (nextTrack.track == tts)
			{
				return true;
			}
		}
		foreach (ConnectedTrackInfo prevTrack in prevTracks)
		{
			if (prevTrack.track == tts)
			{
				return true;
			}
		}
		return false;
	}

	private static Vector3 GetInitialVector(TrainTrackSpline track, TrackPosition p, TrackOrientation o)
	{
		Vector3 position;
		Vector3 position2;
		if (p == TrackPosition.Next)
		{
			if (o == TrackOrientation.Reverse)
			{
				position = track.points[track.points.Length - 1];
				position2 = track.points[track.points.Length - 2];
			}
			else
			{
				position = track.points[0];
				position2 = track.points[1];
			}
		}
		else if (o == TrackOrientation.Reverse)
		{
			position = track.points[1];
			position2 = track.points[0];
		}
		else
		{
			position = track.points[track.points.Length - 2];
			position2 = track.points[track.points.Length - 1];
		}
		return track.transform.TransformPoint(position2) - track.transform.TransformPoint(position);
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		for (int i = 0; i < nextTracks.Count; i++)
		{
			Color splineColour = Color.white;
			if (straightestNextIndex != i && nextTracks.Count > 1)
			{
				if (i == 0)
				{
					splineColour = Color.green;
				}
				else if (i == nextTracks.Count - 1)
				{
					splineColour = Color.yellow;
				}
			}
			WorldSpline.DrawSplineGizmo(nextTracks[i].track, splineColour);
		}
		for (int j = 0; j < prevTracks.Count; j++)
		{
			Color splineColour2 = Color.white;
			if (straightestPrevIndex != j && prevTracks.Count > 1)
			{
				if (j == 0)
				{
					splineColour2 = Color.green;
				}
				else if (j == nextTracks.Count - 1)
				{
					splineColour2 = Color.yellow;
				}
			}
			WorldSpline.DrawSplineGizmo(prevTracks[j].track, splineColour2);
		}
	}

	public ConnectedTrackInfo GetTrackSelection(List<ConnectedTrackInfo> trackOptions, int straightestIndex, TrackSelection trackSelection, bool nextTrack, bool trainForward, TrainTrackSpline preferredAltTrack)
	{
		if (trackOptions.Count == 1)
		{
			return trackOptions[0];
		}
		if (preferredAltTrack != null)
		{
			foreach (ConnectedTrackInfo trackOption in trackOptions)
			{
				if (trackOption.track == preferredAltTrack)
				{
					return trackOption;
				}
			}
		}
		bool flag = nextTrack ^ trainForward;
		switch (trackSelection)
		{
		case TrackSelection.Left:
			if (!flag)
			{
				return trackOptions[0];
			}
			return trackOptions[trackOptions.Count - 1];
		case TrackSelection.Right:
			if (!flag)
			{
				return trackOptions[trackOptions.Count - 1];
			}
			return trackOptions[0];
		default:
			return trackOptions[straightestIndex];
		}
	}

	public static bool TryFindTrackNearby(Vector3 pos, float maxDist, out TrainTrackSpline splineResult, out float distResult)
	{
		splineResult = null;
		distResult = 0f;
		List<Collider> obj = Pool.GetList<Collider>();
		GamePhysics.OverlapSphere(pos, maxDist, obj, 65536);
		if (obj.Count > 0)
		{
			List<TrainTrackSpline> obj2 = Pool.GetList<TrainTrackSpline>();
			float num = float.MaxValue;
			foreach (Collider item in obj)
			{
				item.GetComponentsInParent(includeInactive: false, obj2);
				if (obj2.Count <= 0)
				{
					continue;
				}
				foreach (TrainTrackSpline item2 in obj2)
				{
					float minSplineDist;
					float distance = item2.GetDistance(pos, 1f, out minSplineDist);
					if (distance < num)
					{
						num = distance;
						distResult = minSplineDist;
						splineResult = item2;
					}
				}
			}
			Pool.FreeList(ref obj2);
		}
		Pool.FreeList(ref obj);
		return splineResult != null;
	}
}
