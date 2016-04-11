using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;


namespace ScrumFactory.Composition {

    public interface IConfigValue {        
        Object Value { get; set; }
    }


    [Export]
    public class Configuration {

        [ImportMany(typeof(IConfigValue))]
        private Lazy<IConfigValue, IDictionary<string, object>>[] configs = null;

        [Import]
        private IEventAggregator aggregator { get; set; }
        
        public IConfigValue Get(string name) {

            if (configs == null)
                return null;
            
            var config = configs.FirstOrDefault(c => c.Metadata["Name"].ToString() == name);
            
            if (config==null)
                return null;

            return config.Value;
        }

        public int GetIntValue(string name) {
            var config = Get(name);
            if (config == null)
                return 0;
            return (int)config.Value;
        }

        public bool GetBoolValue(string name) {
            var config = Get(name);
            if (config == null)
                return false;
            return (bool)config.Value;
        }

        public string GetStringValue(string name) {
            var config = Get(name);
            if (config == null)
                return null;
            return (string)config.Value;
        }

        public void SetValue(string name, object value) {
            IConfigValue config = Get(name);
            if (config == null)
                return;
            config.Value = value;

            aggregator.Publish<string>(ScrumFactoryEvent.ConfigChanged, name);
        }
    }
}
