﻿using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EventVisualizer.Base
{
    [System.Serializable]
    public class EventCall
    {
        public Object Sender { get; private set; }
        public Object Receiver { get; private set; }
        public string EventName { get; private set; }
        public string Method { get; private set; }
		public string ReceiverComponentName { get; private set; }
		public string ReceiverComponentNameSimple { get; private set; }

		public NodeData nodeSender;
        public NodeData nodeReceiver;
        public double lastTimeExecuted { get; private set; }
        public int timesExecuted { get; private set; }
		public readonly Color color;

        public string MethodFullPath
        {
            get
            {
                return ReceiverComponentName + "." + Method;
            }
        }

        public System.Action OnTriggered;

        private static Regex parenteshesPattern = new Regex(@"\(([^\(]*)\)$");

        public EventCall(Object sender, Object receiver, string eventName, string methodName, UnityEventBase unityEvent)
        {
            Sender = sender as Component ? (sender as Component).gameObject : sender;
            Receiver = receiver as Component ? (receiver as Component).gameObject : receiver;
            EventName = eventName;
            Method = methodName;
			color = EdgeGUI.ColorForIndex(Animator.StringToHash(eventName));

            UpdateReceiverComponentName(receiver);
            AttachTrigger(unityEvent);
        }

        private void AttachTrigger(UnityEventBase unityEvent)
        {
            if (unityEvent == null)
            {
                return;
            }
            MethodInfo eventRegisterMethod = unityEvent.GetType().GetMethod("AddListener");
            if (eventRegisterMethod != null)
            {
                System.Type eventType = eventRegisterMethod.GetParameters()[0].ParameterType;
                ParameterInfo[] eventParameters = eventType.GetMethod("Invoke").GetParameters(); 
                
                if (eventParameters.Length == 0)
                {
                    MethodInfo methodInfo = this.GetType()
                        .GetMethod("TriggerZeroArgs", BindingFlags.Public | BindingFlags.Instance);

                    System.Type actionT = typeof(UnityAction);
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        triggerAction
                    });
                }

                else if (eventParameters.Length == 1)
                {
                    System.Type t0 = eventParameters[0].ParameterType;

                    MethodInfo methodInfo = this.GetType()
                        .GetMethod("TriggerOneArg", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(t0);

                    System.Type actionT = typeof(UnityAction<>).MakeGenericType(t0);
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        triggerAction
                    });
                }
                else if (eventParameters.Length == 2)
                {
                    System.Type t0 = eventParameters[0].ParameterType;
                    System.Type t1 = eventParameters[1].ParameterType;

                    MethodInfo methodInfo = this.GetType()
                        .GetMethod("TriggerTwoArgs", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(t0, t1);

                    System.Type actionT = typeof(UnityAction<,>).MakeGenericType(t0, t1);
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        triggerAction
                    });
                }
                else if (eventParameters.Length == 3)
                {
                    System.Type t0 = eventParameters[0].ParameterType;
                    System.Type t1 = eventParameters[1].ParameterType;
                    System.Type t2 = eventParameters[2].ParameterType;

                    MethodInfo methodInfo = this.GetType()
                        .GetMethod("TriggerThreeArgs", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(t0, t1,t2);

                    System.Type actionT = typeof(UnityAction<,,>).MakeGenericType(t0, t1,t2);
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        triggerAction
                    });
                }
                else if (eventParameters.Length == 2)
                {
                    System.Type t0 = eventParameters[0].ParameterType;
                    System.Type t1 = eventParameters[1].ParameterType;
                    System.Type t2 = eventParameters[2].ParameterType;
                    System.Type t3 = eventParameters[3].ParameterType;

                    MethodInfo methodInfo = this.GetType()
                        .GetMethod("TriggerFourArgs", BindingFlags.Public | BindingFlags.Instance)
                        .MakeGenericMethod(t0, t1,t2,t3);

                    System.Type actionT = typeof(UnityAction<,,,>).MakeGenericType(t0, t1, t2,t3);
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        triggerAction
                    });
                }
            }
        }

        #region generic callers
        public void TriggerZeroArgs()
        {
            OnExecuted();
            if (OnTriggered != null)
            {
                OnTriggered.Invoke();
            }
        }

        public void TriggerOneArg<T0>(T0 arg0)
        {
            OnExecuted();
            if (OnTriggered != null)
            {
                OnTriggered.Invoke();
            }
        }
        public void TriggerTwoArgs<T0,T1>(T0 arg0, T1 arg1)
        {
            OnExecuted();
            if (OnTriggered != null)
            {
                OnTriggered.Invoke();
            }
        }
        public void TriggerThreeArgs<T0,T1,T2>(T0 arg,T1 arg1, T2 arg2)
        {
            OnExecuted();
            if (OnTriggered != null)
            {
                OnTriggered.Invoke();
            }
        }
        public void TriggerFourArgs<T0, T1, T2, T3>(T0 arg, T1 arg1, T2 arg2, T3 arg3)
        {
            OnExecuted();
            if (OnTriggered != null)
            {
                OnTriggered.Invoke();
            }
        }

        private void OnExecuted() {
            timesExecuted++;
            lastTimeExecuted = EditorApplication.timeSinceStartup;
        }
        #endregion

        private void UpdateReceiverComponentName(Object component)
        {
            if (Receiver != null)
            {
				Debug.Log(component.ToString());
                MatchCollection matches = parenteshesPattern.Matches(component.ToString());
                if (matches != null && matches.Count == 1)
                {
                    ReceiverComponentName = matches[0].Value;
                    ReceiverComponentName = ReceiverComponentName.Substring(1, ReceiverComponentName.Length - 2);
					int lastDot = ReceiverComponentName.LastIndexOf('.') + 1;
					ReceiverComponentNameSimple = ReceiverComponentName.Substring(lastDot, ReceiverComponentName.Length - lastDot);
				}
            }
        }
    }
}