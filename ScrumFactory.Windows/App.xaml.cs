using System.ComponentModel.Composition.Extensions;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Windows.Markup;
using System.Reflection;
using System.Windows;
using System.Deployment.Application;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Text.RegularExpressions;


namespace ScrumFactory.Windows {
    

    public partial class App : Application {

        private ViewModel.ShellViewModel shellViewModel;
        private ApplicationInstanceMonitor<string> instanceMonitor;


        public CompositionContainer Container { get; private set; }

        private Services.ILogService log;
        
        public App() {
            
            // ay 4.5.1 the textboxes stop to acepting , character
            // this fix it
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            this.instanceMonitor = new ApplicationInstanceMonitor<string>();
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            this.Startup += new StartupEventHandler(AppStartup);
            this.Exit += new ExitEventHandler(App_Exit);
        }

        void App_Exit(object sender, ExitEventArgs e) {

            if (shellViewModel != null)
                shellViewModel.SaveRecentProjects();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            
            // if has no shell yet throws it anyway
            if (shellViewModel == null)
                return;

            // tries to get it as a Scrum factory exceptions
            ScrumFactory.Exceptions.ScrumFactoryException sfEx = null;
            if (e.Exception is ScrumFactory.Exceptions.ScrumFactoryException)
                sfEx = e.Exception as ScrumFactory.Exceptions.ScrumFactoryException;
            if (e.Exception.InnerException is ScrumFactory.Exceptions.ScrumFactoryException)
                sfEx = e.Exception.InnerException as ScrumFactory.Exceptions.ScrumFactoryException;

            if (e.Exception is System.Net.Http.HttpRequestException)
                sfEx = new Exceptions.NetworkException();
           


            // if is not a factoy exception, throws it to the OS
            if (sfEx==null) {          
                if(log!=null)
                    log.LogError(e.Exception);
                return;
            }

            // if is a ScrumFactoryExcpetion let the shell handle it            
            shellViewModel.HandleScrumFactoryException(sfEx);
            e.Handled = true;
        }


        private void AppStartup(object sender, StartupEventArgs e) {
            
            if (!instanceMonitor.Assert()) {
                
                // Defer to another instance.
                instanceMonitor.NotifyNewInstance(GetQueryString());
                this.Shutdown();
                return;
            }

            instanceMonitor.NewInstanceCreated += OnNewInstanceCreated;

            SetFrameworkElementToUserCulture();

            string deployPath = Assembly.GetExecutingAssembly().Location.Replace("ScrumFactory.Windows.exe", "");
            
            SetupSpellCheck(deployPath);

            SetupStandAloneServerRoot(deployPath);
            SetupStandAloneDB(deployPath);

            var catalog = new AggregateCatalog();
            
            catalog.Catalogs.Add(new DirectoryCatalog(deployPath, "ScrumFactory.*.dll"));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string pluginPath = myDocumentsPath + "\\Scrum Factory\\Plugins";
            if (System.IO.Directory.Exists(pluginPath))
                catalog.Catalogs.Add(new DirectoryCatalog(pluginPath, "*.dll"));
            else {
                try {
                    System.IO.Directory.CreateDirectory(pluginPath);
                }
                catch (Exception) { }
            }

           


            catalog.Catalogs.Add(new ConfigurationCatalog());

            Container = new CompositionContainer(catalog);
                                    
            this.shellViewModel = Container.GetExport<ScrumFactory.Windows.ViewModel.ShellViewModel>().Value;

            this.log = Container.GetExport<Services.ILogService>().Value;

            // starts all view models with the StartsWithApp interface
            Container.GetExportedValues<ScrumFactory.Composition.ViewModel.IStartsWithApp>();

            var args = ParseQueryString(GetQueryString());  
            
            this.shellViewModel.Show(args);

            

        }

        private void SetupStandAloneServerRoot(string deployPath) {

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            try {
                if (File.Exists(deployPath + "_localServerRoot.zip")) {
                    using (var zip = ZipFile.OpenRead(deployPath + "_localServerRoot.zip")) {
                        foreach (ZipArchiveEntry file in zip.Entries) {

                            var fileFullname = Path.Combine(deployPath, file.FullName);
                            var path = Path.GetDirectoryName(fileFullname);
                
                            if(!Directory.Exists(path)) {
                                Directory.CreateDirectory(path);
                            }
                            
                            // dont want to overwrite log and database
                            var fileName = Path.GetFileName(fileFullname);
                            if (!String.IsNullOrEmpty(fileName) && !file.FullName.Contains("App_Data")) {
                                file.ExtractToFile(fileFullname, true);
                                if(fileName=="Web.config") {
                                    File.WriteAllText(fileFullname, Regex.Replace(File.ReadAllText(fileFullname), "\\|DataDirectory\\|", localAppData));
                                }
                            }
                        }
                    }

                    

                    System.IO.File.Delete(deployPath + "_localServerRoot.zip");
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("ERROR CREATING LOCAL SERVER\n" + ex.Message);
            }
        }

        private void SetupStandAloneDB(string deployPath) {


            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            try {
                if (!System.IO.File.Exists(localAppData + "\\ScrumFactory.mdf")) {


                    System.IO.File.Copy(deployPath + "ScrumFactoryEMPTY.mdf", localAppData + "\\ScrumFactory.mdf");
                    System.IO.File.Copy(deployPath + "ScrumFactory_logEMPTY.ldf", localAppData + "\\ScrumFactory_log.ldf");

                    System.IO.File.Delete(deployPath + "ScrumFactoryEMPTY.mdf");
                    System.IO.File.Delete(deployPath + "ScrumFactory_logEMPTY.ldf");
                }
            } catch (Exception) { }
        }

        private void SetupSpellCheck(string deployPath) {
            string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string dicsPath = myDocumentsPath + "\\Scrum Factory\\SpellDics\\";


            if (!System.IO.Directory.Exists(dicsPath)) {
                try {
                    // create dics folder
                    System.IO.Directory.CreateDirectory(dicsPath);
                    // copy dics
                    var files = System.IO.Directory.GetFileSystemEntries(deployPath + "SpellDics");
                    foreach(var f in files) {
                        System.IO.File.Copy(f, dicsPath + System.IO.Path.GetFileName(f), false);
                    }
                } catch (Exception) { }
            }
            
            // if no language defined yet
            if (String.IsNullOrEmpty(ScrumFactory.Windows.Properties.Settings.Default.SpellCheckLanguage)) {
                string systemLang = System.Globalization.CultureInfo.CurrentCulture.Name.Replace("-", "_");
                if (!System.IO.File.Exists(dicsPath + systemLang + ".dic") || !System.IO.File.Exists(dicsPath + systemLang + ".aff"))
                    ScrumFactory.Windows.Properties.Settings.Default.SpellCheckLanguage = "-";
                else
                    ScrumFactory.Windows.Properties.Settings.Default.SpellCheckLanguage = systemLang;
                ScrumFactory.Windows.Properties.Settings.Default.Save();
            }

            System.Windows.KExtensions.SpellCheck.DefaultDicPath = dicsPath;
            System.Windows.KExtensions.SpellCheck.DefaultLanguage = ScrumFactory.Windows.Properties.Settings.Default.SpellCheckLanguage;
            if (System.Windows.KExtensions.SpellCheck.DefaultLanguage == "-")
                System.Windows.KExtensions.SpellCheck.DisableAll = true;

            

        }

        public static NameValueCollection ParseQueryString(string s) {
            NameValueCollection nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?")) {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in  System.Text.RegularExpressions.Regex.Split(s, "&")) {
                string[] singlePair = System.Text.RegularExpressions.Regex.Split(vp, "=");
                if (singlePair.Length == 2) {
                    nvc.Add(singlePair[0], singlePair[1]);
                } else {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }

        private void OnNewInstanceCreated(object sender, NewInstanceCreatedEventArgs<string> e) {
            var args = ParseQueryString(e.Message);

            this.shellViewModel.Show(args);

            
        }

        public string GetQueryString() {
            if (!ApplicationDeployment.IsNetworkDeployed || ApplicationDeployment.CurrentDeployment.ActivationUri == null) {
                string[] args = Environment.GetCommandLineArgs();                
                if (args.Length > 1)
                    return args[1];
                else
                    return string.Empty;                
            }
            return
                ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                
        }

        /// <summary>
        /// Sets the Culture to the current user culture.
        /// </summary>        
        private void SetFrameworkElementToUserCulture() {

            // gets setup language
            string language = ScrumFactory.Windows.Properties.Settings.Default.Language;

            // if null, uses SO language
            if (String.IsNullOrEmpty(language)) {
                language = CultureInfo.CurrentUICulture.IetfLanguageTag;
            }

            // setups language
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(language)));
        }

        public Thickness SystemBorderThickness {
            get {
                double taskBarHeight = SystemParameters.VirtualScreenHeight - SystemParameters.WorkArea.Height + System.Windows.SystemParameters.ResizeFrameHorizontalBorderHeight;                
                return new Thickness(
                    SystemParameters.ResizeFrameVerticalBorderWidth -2,
                    SystemParameters.ResizeFrameHorizontalBorderHeight -1,
                    SystemParameters.ResizeFrameVerticalBorderWidth -3,
                    taskBarHeight -2);
            }
        }




      
    }


}
