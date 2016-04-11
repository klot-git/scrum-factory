using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ScrumFactory.Composition {
    
    [Export(typeof(IEventAggregator))]
    public class EventAggregator : IEventAggregator {

        private Dictionary<string, List<Event>> events = new Dictionary<string, List<Event>>();

        private Dictionary<string, List<Event>> eventsNoData = new Dictionary<string, List<Event>>();
        
        public void UnSubscribeAll(object obj) {
            
            foreach (List<Event> es in events.Values) {
                Event[] objSubs = es.Where(e => e.Object == obj).ToArray();
                for (int i = 0; i < objSubs.Length; i++)
                    es.Remove(objSubs[i]);
            }

            foreach (List<Event> es in eventsNoData.Values) {
                Event[] objSubs = es.Where(e => e.Object == obj).ToArray();
                for (int i = 0; i < objSubs.Length; i++)
                    es.Remove(objSubs[i]);
            }
        }

        public void Subscribe<TData>(ScrumFactoryEvent e, Action<TData> action, int priority) {
            Subscribe<TData>(e.ToString(), action, priority);
        }

        public void Subscribe<TData>(ScrumFactoryEvent e, Action<TData> action) {
            Subscribe<TData>(e.ToString(), action);
        }

        public void Subscribe(ScrumFactoryEvent e, Action action, int priority) {
            Subscribe(e.ToString(), action, priority);
        }

        public void Subscribe(ScrumFactoryEvent e, Action action) {
            Subscribe(e.ToString(), action);
        }

        public void Subscribe<TData>(string eventName, Action<TData> action) {
            Subscribe<TData>(eventName, action, 0);
        }
        
        public void Subscribe<TData>(string eventName, Action<TData> action, int priority) {
            if (!events.ContainsKey(eventName)){
                events[eventName] = new List<Event>();
            }

            var namedEvent = events[eventName];

            namedEvent.Add(new Event(d => { action((TData)d);}, action.Target, priority));
        }

        public void Subscribe(string eventName, Action action) {
            Subscribe(eventName, action, 0);
        }

        public void Subscribe(string eventName, Action action, int priority) {
            if (!eventsNoData.ContainsKey(eventName)) {
                eventsNoData[eventName] = new List<Event>();
            }

            var namedEvent = eventsNoData[eventName];

            namedEvent.Add(new Event(action, priority));
        }

        public void Publish<TData>(ScrumFactoryEvent e, TData data) {
            Publish<TData>(e.ToString(), data);
        }

        public void Publish(ScrumFactoryEvent e) {
            Publish(e.ToString());
        }

        public void Publish<TData>(string eventName, TData data)
        {
            if (events.ContainsKey(eventName)) {
                foreach(Event e in events[eventName].OrderByDescending(e => e.Priority)) {
                    if (e.Object!=null)
                        e.Invoke(data);     
                }
            }
        }

        public void Publish(string eventName) {
            if (eventsNoData.ContainsKey(eventName)) {                
                foreach(Event e in eventsNoData[eventName]) {
                    if(e.Object!=null)
                        e.Invoke();                         
                }                
            }
        }

        public class Event {

            //public System.Reflection.MethodInfo Method { get; private set; }
            //private WeakReference objectRef;

            private Action actionNoData;
            private Action<object> action;

            public object Object { get; private set; }

            public Event(Action<object> action, object target, int priority) {
                this.action = action;
                Object = target;
                Priority = priority;
            }

            public Event(Action<object> action, object target) {
                this.action = action;
                Object = target;
            }

            public Event(Action action) {
                this.actionNoData = action;
                Object = action.Target;
            }

            public Event(Action action, int priority) {
                this.actionNoData = action;
                Object = action.Target;
                Priority = priority;
            }

            public void Invoke() {
                if (actionNoData != null)
                    actionNoData.Invoke();
            }

            public void Invoke(object data) {
                if (action != null)
                    action.Invoke(data);
            }

            public int Priority { get; private set; }


        }


                
    }



}
