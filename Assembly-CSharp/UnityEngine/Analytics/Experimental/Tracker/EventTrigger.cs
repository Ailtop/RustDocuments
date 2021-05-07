using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class EventTrigger
	{
		internal delegate void OnTrigger();

		[SerializeField]
		private TriggerType m_Type;

		[SerializeField]
		private TriggerLifecycleEvent m_LifecycleEvent;

		[SerializeField]
		private bool m_ApplyRules;

		[SerializeField]
		private TriggerListContainer m_Rules;

		[SerializeField]
		private TriggerBool m_TriggerBool;

		[SerializeField]
		private float m_InitTime = 5f;

		[SerializeField]
		private float m_RepeatTime = 5f;

		[SerializeField]
		private int m_Repetitions;

		public int repetitionCount;

		private OnTrigger m_TriggerFunction;

		[SerializeField]
		private TriggerMethod m_Method;

		public TriggerType triggerType => m_Type;

		public TriggerLifecycleEvent lifecycleEvent => m_LifecycleEvent;

		public float initTime
		{
			get
			{
				return m_InitTime;
			}
			set
			{
				m_InitTime = value;
			}
		}

		public float repeatTime
		{
			get
			{
				return m_RepeatTime;
			}
			set
			{
				m_RepeatTime = value;
			}
		}

		public int repetitions
		{
			get
			{
				return m_Repetitions;
			}
			set
			{
				m_Repetitions = value;
			}
		}

		public EventTrigger()
		{
			m_Rules = new TriggerListContainer();
		}

		public void AddRule()
		{
			TriggerRule item = new TriggerRule();
			m_Rules.rules.Add(item);
		}

		public void RemoveRule(int index)
		{
			m_Rules.rules.RemoveAt(index);
		}

		public bool Test(GameObject gameObject = null)
		{
			if (!m_ApplyRules)
			{
				return true;
			}
			if (repetitions > 0 && repetitionCount >= repetitions)
			{
				return false;
			}
			bool flag = false;
			int num = 0;
			int num2 = 0;
			foreach (TriggerRule rule in m_Rules.rules)
			{
				num2++;
				bool error;
				string message;
				if (rule.Test(out error, out message))
				{
					num++;
				}
				else if (error)
				{
					Debug.LogWarningFormat("Event trigger rule {0}{2} is incomplete ({1}). Result is false.", num2, message, (gameObject == null) ? null : $" on GameObject '{gameObject.name}'");
				}
				switch (m_TriggerBool)
				{
				case TriggerBool.All:
					if (num < num2)
					{
						flag = false;
					}
					break;
				case TriggerBool.None:
					if (num > 0)
					{
						flag = false;
					}
					break;
				case TriggerBool.Any:
					if (num > 0)
					{
						flag = true;
					}
					break;
				}
			}
			if ((m_TriggerBool == TriggerBool.All && num == num2) || (m_TriggerBool == TriggerBool.None && num == 0))
			{
				flag = true;
			}
			if (repetitions > 0 && flag)
			{
				repetitionCount++;
			}
			return flag;
		}
	}
}
