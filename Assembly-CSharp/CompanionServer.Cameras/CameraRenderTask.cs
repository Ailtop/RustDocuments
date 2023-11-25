using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

public class CameraRenderTask : CustomYieldInstruction, IDisposable
{
	public const int MaxSamplesPerRender = 10000;

	public const int MaxColliders = 512;

	private static readonly Dictionary<(int, int), NativeArray<int2>> _samplePositions = new Dictionary<(int, int), NativeArray<int2>>();

	private NativeArray<RaycastCommand> _raycastCommands;

	private NativeArray<RaycastHit> _raycastHits;

	private NativeArray<int> _colliderIds;

	private NativeArray<byte> _colliderMaterials;

	private NativeArray<int> _colliderHits;

	private NativeArray<int> _raycastOutput;

	private NativeArray<int> _foundCollidersLength;

	private NativeArray<int> _foundColliders;

	private NativeArray<int> _outputDataLength;

	private NativeArray<byte> _outputData;

	private JobHandle? _pendingJob;

	private int _sampleCount;

	private int _colliderLength;

	public override bool keepWaiting
	{
		get
		{
			if (_pendingJob.HasValue)
			{
				return !_pendingJob.Value.IsCompleted;
			}
			return false;
		}
	}

	public CameraRenderTask()
	{
		_raycastCommands = new NativeArray<RaycastCommand>(10000, Allocator.Persistent);
		_raycastHits = new NativeArray<RaycastHit>(10000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_colliderIds = new NativeArray<int>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_colliderMaterials = new NativeArray<byte>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_colliderHits = new NativeArray<int>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_raycastOutput = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_foundCollidersLength = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_foundColliders = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_outputDataLength = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		_outputData = new NativeArray<byte>(40000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	~CameraRenderTask()
	{
		Dispose();
	}

	public void Dispose()
	{
		_raycastCommands.Dispose();
		_raycastHits.Dispose();
		_colliderIds.Dispose();
		_colliderMaterials.Dispose();
		_colliderHits.Dispose();
		_raycastOutput.Dispose();
		_foundCollidersLength.Dispose();
		_foundColliders.Dispose();
		_outputDataLength.Dispose();
		_outputData.Dispose();
	}

	public new void Reset()
	{
		if (_pendingJob.HasValue)
		{
			if (!_pendingJob.Value.IsCompleted)
			{
				Debug.LogWarning("CameraRenderTask is resetting before completion! This will cause it to synchronously block for completion.");
			}
			_pendingJob.Value.Complete();
		}
		_pendingJob = null;
		_sampleCount = 0;
	}

	public int Start(int width, int height, float verticalFov, float nearPlane, float farPlane, int layerMask, Transform cameraTransform, int sampleCount, int sampleOffset, Dictionary<int, (byte MaterialIndex, int Age)> knownColliders)
	{
		if (cameraTransform == null)
		{
			throw new ArgumentNullException("cameraTransform");
		}
		if (sampleCount <= 0 || sampleCount > 10000)
		{
			throw new ArgumentOutOfRangeException("sampleCount");
		}
		if (sampleOffset < 0)
		{
			throw new ArgumentOutOfRangeException("sampleOffset");
		}
		if (knownColliders == null)
		{
			throw new ArgumentNullException("knownColliders");
		}
		if (knownColliders.Count > 512)
		{
			throw new ArgumentException("Too many colliders", "knownColliders");
		}
		if (_pendingJob.HasValue)
		{
			throw new InvalidOperationException("A render job was already started for this instance.");
		}
		_sampleCount = sampleCount;
		_colliderLength = knownColliders.Count;
		int num = 0;
		foreach (KeyValuePair<int, (byte, int)> knownCollider in knownColliders)
		{
			_colliderIds[num] = knownCollider.Key;
			_colliderMaterials[num] = knownCollider.Value.Item1;
			num++;
		}
		NativeArray<int2> samplePositions = GetSamplePositions(width, height);
		_foundCollidersLength[0] = 0;
		RaycastBufferSetupJob raycastBufferSetupJob = default(RaycastBufferSetupJob);
		raycastBufferSetupJob.colliderIds = _colliderIds.GetSubArray(0, _colliderLength);
		raycastBufferSetupJob.colliderMaterials = _colliderMaterials.GetSubArray(0, _colliderLength);
		raycastBufferSetupJob.colliderHits = _colliderHits.GetSubArray(0, _colliderLength);
		RaycastBufferSetupJob jobData = raycastBufferSetupJob;
		RaycastRaySetupJob raycastRaySetupJob = default(RaycastRaySetupJob);
		raycastRaySetupJob.res = new float2(width, height);
		raycastRaySetupJob.halfRes = new float2((float)width / 2f, (float)height / 2f);
		raycastRaySetupJob.aspectRatio = (float)width / (float)height;
		raycastRaySetupJob.worldHeight = 2f * Mathf.Tan(MathF.PI / 360f * verticalFov);
		raycastRaySetupJob.cameraPos = cameraTransform.position;
		raycastRaySetupJob.cameraRot = cameraTransform.rotation;
		raycastRaySetupJob.nearPlane = nearPlane;
		raycastRaySetupJob.farPlane = farPlane;
		raycastRaySetupJob.layerMask = layerMask;
		raycastRaySetupJob.samplePositions = samplePositions;
		raycastRaySetupJob.sampleOffset = sampleOffset % samplePositions.Length;
		raycastRaySetupJob.raycastCommands = _raycastCommands.GetSubArray(0, sampleCount);
		RaycastRaySetupJob jobData2 = raycastRaySetupJob;
		RaycastRayProcessingJob raycastRayProcessingJob = default(RaycastRayProcessingJob);
		raycastRayProcessingJob.cameraForward = -cameraTransform.forward;
		raycastRayProcessingJob.farPlane = farPlane;
		raycastRayProcessingJob.raycastHits = _raycastHits.GetSubArray(0, sampleCount);
		raycastRayProcessingJob.colliderIds = _colliderIds.GetSubArray(0, _colliderLength);
		raycastRayProcessingJob.colliderMaterials = _colliderMaterials.GetSubArray(0, _colliderLength);
		raycastRayProcessingJob.colliderHits = _colliderHits.GetSubArray(0, _colliderLength);
		raycastRayProcessingJob.outputs = _raycastOutput.GetSubArray(0, sampleCount);
		raycastRayProcessingJob.foundCollidersIndex = _foundCollidersLength;
		raycastRayProcessingJob.foundColliders = _foundColliders;
		RaycastRayProcessingJob jobData3 = raycastRayProcessingJob;
		RaycastColliderProcessingJob raycastColliderProcessingJob = default(RaycastColliderProcessingJob);
		raycastColliderProcessingJob.foundCollidersLength = _foundCollidersLength;
		raycastColliderProcessingJob.foundColliders = _foundColliders;
		RaycastColliderProcessingJob jobData4 = raycastColliderProcessingJob;
		RaycastOutputCompressJob raycastOutputCompressJob = default(RaycastOutputCompressJob);
		raycastOutputCompressJob.rayOutputs = _raycastOutput.GetSubArray(0, sampleCount);
		raycastOutputCompressJob.dataLength = _outputDataLength;
		raycastOutputCompressJob.data = _outputData;
		RaycastOutputCompressJob jobData5 = raycastOutputCompressJob;
		JobHandle job = jobData.Schedule();
		JobHandle job2 = RaycastCommand.ScheduleBatch(dependsOn: jobData2.Schedule(sampleCount, 100), commands: _raycastCommands.GetSubArray(0, sampleCount), results: _raycastHits.GetSubArray(0, sampleCount), minCommandsPerJob: 100);
		JobHandle dependsOn2 = jobData3.Schedule(sampleCount, 100, JobHandle.CombineDependencies(job, job2));
		JobHandle job3 = jobData4.Schedule(dependsOn2);
		JobHandle job4 = jobData5.Schedule(dependsOn2);
		_pendingJob = JobHandle.CombineDependencies(job4, job3);
		return sampleOffset + sampleCount;
	}

	public int ExtractRayData(byte[] buffer, List<int> hitColliderIds = null, List<int> foundColliderIds = null)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		int num = _sampleCount * 4;
		if (buffer.Length < num)
		{
			throw new ArgumentException("Output buffer is not large enough to hold all the ray data", "buffer");
		}
		if (!_pendingJob.HasValue)
		{
			throw new InvalidOperationException("Job was not started for this CameraRenderTask");
		}
		if (!_pendingJob.Value.IsCompleted)
		{
			Debug.LogWarning("Trying to extract ray data from CameraRenderTask before completion! This will cause it to synchronously block for completion.");
		}
		_pendingJob.Value.Complete();
		int num2 = _outputDataLength[0];
		NativeArray<byte>.Copy(_outputData.GetSubArray(0, num2), buffer, num2);
		if (hitColliderIds != null)
		{
			hitColliderIds.Clear();
			for (int i = 0; i < _colliderLength; i++)
			{
				if (_colliderHits[i] > 0)
				{
					hitColliderIds.Add(_colliderIds[i]);
				}
			}
		}
		if (foundColliderIds != null)
		{
			foundColliderIds.Clear();
			int num3 = _foundCollidersLength[0];
			for (int j = 0; j < num3; j++)
			{
				foundColliderIds.Add(_foundColliders[j]);
			}
		}
		return num2;
	}

	private static NativeArray<int2> GetSamplePositions(int width, int height)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		(int, int) key = (width, height);
		if (_samplePositions.TryGetValue(key, out var value))
		{
			return value;
		}
		value = new NativeArray<int2>(width * height, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		RaycastSamplePositionsJob jobData = default(RaycastSamplePositionsJob);
		jobData.res = new int2(width, height);
		jobData.random = new Unity.Mathematics.Random(1337u);
		jobData.positions = value;
		jobData.Run();
		_samplePositions.Add(key, value);
		return value;
	}

	public static void FreeCachedSamplePositions()
	{
		foreach (KeyValuePair<(int, int), NativeArray<int2>> samplePosition in _samplePositions)
		{
			samplePosition.Value.Dispose();
		}
		_samplePositions.Clear();
	}
}
