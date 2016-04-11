namespace System.ComponentModel.Composition.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Primitives;
    using System.Configuration;
    using System.Linq;

    public class ConnectionStringCatalog : ComposablePartCatalog
    {
        public override System.Linq.IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return ConfigurationManager.ConnectionStrings.Cast<System.Configuration.ConnectionStringSettings>().Select(c => new ConnectionStringPartDefinition(c.Name)).AsQueryable<ComposablePartDefinition>();
            }
        }
    }

    public class ConnectionStringComposablePart : ComposablePart
    {
        private string key;

        public ConnectionStringComposablePart(string key)
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
            return ConfigurationManager.ConnectionStrings[definition.ContractName].ConnectionString;
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

    public class ConnectionStringPartDefinition : ComposablePartDefinition
    {
        private string key;

        public ConnectionStringPartDefinition(string key)
        {
            this.key = key;
        }
        public override ComposablePart CreatePart()
        {
            return new ConnectionStringComposablePart(this.key);
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