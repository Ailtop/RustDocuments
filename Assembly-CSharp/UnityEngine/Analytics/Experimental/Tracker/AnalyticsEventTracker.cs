using System.Collections;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[AddComponentMenu("Analytics/Experimental/Analytics Event Tracker")]
	public class AnalyticsEventTracker : MonoBehaviour
	{
		[SerializeField]
		public EventTrigger m_Trigger = new EventTrigger();

		[SerializeField]
		private StandardEventPayload m_EventPayload = new StandardEventPayload();

		public StandardEventPayload payload => m_EventPayload;

		public void TriggerEvent()
		{
			SendEvent();
		}

		private AnalyticsResult SendEvent()
		{
			if (m_Trigger.Test(base.gameObject))
			{
				return payload.Send();
			}
			return AnalyticsResult.Ok;
		}

		private void Awake()
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.Awake)
			{
				SendEvent();
			}
		}

		private void Start()
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.Start)
			{
				SendEvent();
			}
			else if (m_Trigger.triggerType == TriggerType.Timer)
			{
				StartCoroutine(TimedTrigger());
			}
		}

		private void OnEnable()
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnEnable)
			{
				SendEvent();
			}
		}

		private void OnDisable()
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnDisable)
			{
				SendEvent();
			}
		}

		private void OnApplicationPause(bool paused)
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle)
			{
				if (paused && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnApplicationPause)
				{
					SendEvent();
				}
				else if (!paused && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnApplicationUnpause)
				{
					SendEvent();
				}
			}
		}

		private void OnDestroy()
		{
			if (m_Trigger.triggerType == TriggerType.Lifecycle && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnDestroy)
			{
				SendEvent();
			}
		}

		private IEnumerator TimedTrigger()
		{
			if (m_Trigger.initTime > 0f)
			{
				yield return new WaitForSeconds(m_Trigger.initTime);
			}
			SendEvent();
			while (m_Trigger.repetitions == 0 || m_Trigger.repetitionCount <= m_Trigger.repetitions)
			{
				if (m_Trigger.repeatTime > 0f)
				{
					yield return new WaitForSeconds(m_Trigger.repeatTime);
				}
				else
				{
					yield return null;
				}
				SendEvent();
			}
		}
	}
}
