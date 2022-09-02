using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace _Scripts {
    public class EventManager : StaticInstance<EventManager> {
        private Dictionary<string, UnityEvent<int>> _eventDictionary;
        protected override void Awake() {
            base.Awake();
            Init();
        }

        private void Init() {
            _eventDictionary ??= new Dictionary<string, UnityEvent<int>>();
        }

        public static void StartListening(string eventId, UnityAction<int> listener) {
            // Debug.Log($"Start listening event {eventId}.");
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<int> thisEvent)) {
                thisEvent.AddListener(listener);
            }
            else {
                thisEvent = new UnityEvent<int>();
                thisEvent.AddListener(listener);
                Instance._eventDictionary.Add(eventId, thisEvent);
            }
        }

        public static void StopListening(string eventId, UnityAction<int> listener) {
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<int> thisEvent)) {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(string eventId, int eventParameter) {
            if (Instance._eventDictionary.TryGetValue(eventId, out UnityEvent<int> thisEvent)) {
                Debug.Log($"Event {eventId} is triggered");
                thisEvent.Invoke(eventParameter);
            }
        }
    }
}
