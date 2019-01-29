using System.ComponentModel.Composition;
using System.IO;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Collections.Generic;
using System.Windows;



namespace ScrumFactory.ReportHelper {

    public class ReportConfig {

        public string ReportGroup { get; set; }

        public string ReportTemplate { get; set; }

        public string ReportName { get; set; }

        public List<object> ReportObjects { get; private set; }
        public Dictionary<string, string> ReportVars { get; private set; }
        public Dictionary<string, object> ReportViewModels { get; private set; }

        public string StaticXAMLReport { get; set; }

        public ReportConfig(string group, string template, string name) {
            ReportGroup = group;
            ReportTemplate = template;
            ReportName = name;
            ReportObjects = new List<object>();
            ReportVars = new Dictionary<string, string>();
            ReportViewModels = new Dictionary<string, object>();
        }

        public void AddReportVar(string name, System.DateTime date) {
            ReportVars.Add(name, date.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }


    }

    [Export]
    public class Report  {

        /// <summary>
        /// Creates the report.
        /// </summary>
        public string CreateReportXAML(string serverUrl, ReportConfig config) {

            if (config.StaticXAMLReport != null)
                return config.StaticXAMLReport;

            // load the xslt template
            System.Xml.Xsl.XslCompiledTransform xslt = new System.Xml.Xsl.XslCompiledTransform();
            try {
                LoadTemplate(xslt, serverUrl, config.ReportGroup, config.ReportTemplate);
            }
            catch (System.Exception ex) {
                throw new ScrumFactory.Exceptions.ScrumFactoryException("Error_reading_report_template");
            }

            // creates a buffer stream to write the report context in XML
            System.IO.BufferedStream xmlBuffer = new System.IO.BufferedStream(new System.IO.MemoryStream());
            System.Xml.XmlWriterSettings writerSettings = new System.Xml.XmlWriterSettings();
            writerSettings.CheckCharacters = false;
            writerSettings.OmitXmlDeclaration = true;
            
            System.Xml.XmlWriter reportDataStream = System.Xml.XmlWriter.Create(xmlBuffer, writerSettings);
        
            // write XML start tag
            reportDataStream.WriteStartDocument();
            reportDataStream.WriteStartElement("ReportData");

            // create report context in XML
            CreateDefaultXMLContext(reportDataStream, writerSettings, serverUrl, config);
            
            // finish XML document            
            reportDataStream.WriteEndDocument();
            reportDataStream.Flush();

            xmlBuffer.Seek(0, System.IO.SeekOrigin.Begin);
            // debug
            //System.IO.StreamReader s = new System.IO.StreamReader(xmlBuffer);
            //string ss = s.ReadToEnd();

            System.Xml.XmlReaderSettings readerSettings = new System.Xml.XmlReaderSettings();
            readerSettings.CheckCharacters = false;
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(xmlBuffer, readerSettings);
            
            // creates a buffer stream to write the XAML flow document            
            System.IO.StringWriter xamlBuffer = new System.IO.StringWriter();
            
            System.Xml.XmlWriter xamlWriter = System.Xml.XmlWriter.Create(xamlBuffer, writerSettings);

            // creates the flow document XMAL
            xslt.Transform(xmlReader, xamlWriter);

            // sets the flow document at the view
            return xamlBuffer.ToString();


        }


        private void CreateDefaultXMLContext(System.Xml.XmlWriter reportDataStream, System.Xml.XmlWriterSettings writerSettings, string serverUrl, ReportConfig config) {
            reportDataStream.WriteElementString("Today", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            reportDataStream.WriteElementString("ServerUrl", serverUrl);

            foreach (string key in config.ReportVars.Keys)
                reportDataStream.WriteElementString(key, config.ReportVars[key]);

            foreach (object o in config.ReportObjects)
                SerializeObjectToXML(reportDataStream, writerSettings, o);
        }

        private void LoadTemplate(System.Xml.Xsl.XslCompiledTransform xslt, string serverUrl, string reportGroup, string template) {
            if (!serverUrl.EndsWith("/"))
                serverUrl = serverUrl + "/";
            xslt.Load(serverUrl + "ReportTemplates/" + reportGroup + "/" + template + ".xslt");
        }

        private void SerializeObjectToXML(System.Xml.XmlWriter reportDataStream, System.Xml.XmlWriterSettings writerSettings, object o) {

            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(o.GetType());
            

            System.Xml.Serialization.XmlSerializerNamespaces ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(reportDataStream, writerSettings);
            ser.Serialize(xmlWriter, o, ns);

        }


        public ReportHelper.Paginator CreatePaginator(FlowDocument flowDocument, string title) {
            ReportHelper.Paginator.Definition def = new ReportHelper.Paginator.Definition() {
                Title = title,
                PageSize = new Size(flowDocument.PageWidth, flowDocument.PageHeight),
                Margins = flowDocument.PagePadding,        
            };

            if (flowDocument.Tag==null) {
                def.Header = null;
            }
            else if(!flowDocument.Tag.ToString().Contains("sf-header:yes")) {
                def.Header = null;
            }

            var paginator = new ReportHelper.Paginator(flowDocument, def);
            paginator.ComputePageCount();
            return paginator;
        }


        public void FlowDocumentToXps(DocumentPaginator paginator, string filename, string reportName, bool overWrite) {

            XpsDocument document;
            if (overWrite)
                document = new XpsDocument(filename, FileAccess.Write);
            else
                document = new XpsDocument(filename, FileAccess.ReadWrite);

            XpsPackagingPolicy packagePolicy = new XpsPackagingPolicy(document);
                                    
            XpsSerializationManager serializationMgr = new XpsSerializationManager(packagePolicy, true);

            serializationMgr.Commit();

            serializationMgr.SaveAsXaml(paginator);


            document.Close();

        }



        public void FlowDocumentToPDF(DocumentPaginator paginator, string filename, string reportName, bool overWrite) {

            MemoryStream lMemoryStream = new MemoryStream();
            Package package = Package.Open(lMemoryStream, FileMode.Create);
            XpsDocument doc = new XpsDocument(package);

            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);

            XpsPackagingPolicy packagePolicy = new XpsPackagingPolicy(doc);
            XpsSerializationManager serializationMgr = new XpsSerializationManager(packagePolicy, false);

            writer.Write(paginator);
            doc.Close();
            package.Close();
            
            var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
            PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, filename, 0);
            pdfXpsDoc.Close();
        }
    }
}
