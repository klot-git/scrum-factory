namespace System.ComponentModel.Composition.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Primitives;
    using System.Configuration;
    using System.Linq;

    public class ConfigurationCatalog : ComposablePartCatalog
    {
        public override System.Linq.IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return ConfigurationManager.AppSettings.AllKeys.Select(k => new ConfigurationPartDefinition(k)).AsQueryable<ComposablePartDefinition>();
            }
        }
    }

    public class ConfigurationComposablePart : ComposablePart
    {
        private string key;

        public ConfigurationComposablePart(string key)
        {
            this.key = key;
        }

        public override System.Collections.Generic.IEnumerable<ExportDefinition> ExportDefinitions
        {
            get
            {
                var metadata = new Dictionary<string, object>();
                metadata.Add("ExportTypeIdentity", typeof(string).FullName);

                return new[] { new ExportDefinition(this.key, metadata) };
            }
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            return ConfigurationManager.AppSettings[definition.ContractName];
        }

        public override System.Collections.Generic.IEnumerable<ImportDefinition> ImportDefinitions
        {
            get
            {
                return new ImportDefinition[0];
            }
        }

        public override void SetImport(ImportDefinition definition, System.Collections.Generic.IEnumerable<Export> exports)
        {
        }
    }

    public class ConfigurationPartDefinition : ComposablePartDefinition
    {
        private string key;

        public ConfigurationPartDefinition(string key)
        {
            this.key = key;
        }
        public override ComposablePart CreatePart()
        {
            return new ConfigurationComposablePart(this.key);
        }

        public override System.Collections.Generic.IEnumerable<ExportDefinition> ExportDefinitions
        {
            get
            {

                var metadata = new Dictionary<string, object>();
                metadata.Add("ExportTypeIdentity", typeof(string).FullName);

                return new[] 
                    { 
                        new ExportDefinition(this.key, metadata)
                    };
            }
        }

        public override System.Collections.Generic.IEnumerable<ImportDefinition> ImportDefinitions
        {
            get
            {
                return new ImportDefinition[0];
            }
        }
    }
}