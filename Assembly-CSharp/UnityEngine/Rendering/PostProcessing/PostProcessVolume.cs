using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing
{
	[AddComponentMenu("Rendering/Post-process Volume", 1001)]
	[ExecuteAlways]
	public sealed class PostProcessVolume : MonoBehaviour
	{
		public PostProcessProfile sharedProfile;

		[Tooltip("Check this box to mark this volume as global. This volume's Profile will be applied to the whole Scene.")]
		public bool isGlobal;

		[Min(0f)]
		[Tooltip("The distance (from the attached Collider) to start blending from. A value of 0 means there will be no blending and the Volume overrides will be applied immediatly upon entry to the attached Collider.")]
		public float blendDistance;

		[Range(0f, 1f)]
		[Tooltip("The total weight of this Volume in the Scene. A value of 0 signifies that it will have no effect, 1 signifies full effect.")]
		public float weight = 1f;

		[Tooltip("The volume priority in the stack. A higher value means higher priority. Negative values are supported.")]
		public float priority;

		private int m_PreviousLayer;

		private float m_PreviousPriority;

		private List<Collider> m_TempColliders;

		private PostProcessProfile m_InternalProfile;

		public PostProcessProfile profile
		{
			get
			{
				if (m_InternalProfile == null)
				{
					m_InternalProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
					if (sharedProfile != null)
					{
						foreach (PostProcessEffectSettings setting in sharedProfile.settings)
						{
							PostProcessEffectSettings item = Object.Instantiate(setting);
							m_InternalProfile.settings.Add(item);
						}
					}
				}
				return m_InternalProfile;
			}
			set
			{
				m_InternalProfile = value;
			}
		}

		internal PostProcessProfile profileRef
		{
			get
			{
				if (!(m_InternalProfile == null))
				{
					return m_InternalProfile;
				}
				return sharedProfile;
			}
		}

		public bool HasInstantiatedProfile()
		{
			return m_InternalProfile != null;
		}

		private void OnEnable()
		{
			PostProcessManager.instance.Register(this);
			m_PreviousLayer = base.gameObject.layer;
			m_TempColliders = new List<Collider>();
		}

		private void OnDisable()
		{
			PostProcessManager.instance.Unregister(this);
		}

		private void Update()
		{
			int layer = base.gameObject.layer;
			if (layer != m_PreviousLayer)
			{
				PostProcessManager.instance.UpdateVolumeLayer(this, m_PreviousLayer, layer);
				m_PreviousLayer = layer;
			}
			if (priority != m_PreviousPriority)
			{
				PostProcessManager.instance.SetLayerDirty(layer);
				m_PreviousPriority = priority;
			}
		}

		private void OnDrawGizmos()
		{
			List<Collider> tempColliders = m_TempColliders;
			GetComponents(tempColliders);
			if (isGlobal || tempColliders == null)
			{
				return;
			}
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 vector = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, lossyScale);
			foreach (Collider item in tempColliders)
			{
				if (!item.enabled)
				{
					continue;
				}
				Type type = item.GetType();
				if (type == typeof(BoxCollider))
				{
					BoxCollider boxCollider = (BoxCollider)item;
					Gizmos.DrawCube(boxCollider.center, boxCollider.size);
					Gizmos.DrawWireCube(boxCollider.center, boxCollider.size + vector * blendDistance * 4f);
				}
				else if (type == typeof(SphereCollider))
				{
					SphereCollider sphereCollider = (SphereCollider)item;
					Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
					Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius + vector.x * blendDistance * 2f);
				}
				else if (type == typeof(MeshCollider))
				{
					MeshCollider meshCollider = (MeshCollider)item;
					if (!meshCollider.convex)
					{
						meshCollider.convex = true;
					}
					Gizmos.DrawMesh(meshCollider.sharedMesh);
					Gizmos.DrawWireMesh(meshCollider.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one + vector * blendDistance * 4f);
				}
			}
			tempColliders.Clear();
		}
	}
}
