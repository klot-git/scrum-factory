using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition;

namespace ScrumFactory.Windows
{

    [Export]
    public partial class MainWindow : Window, IDialogView {
        private object model;

       
        private bool IsOnTaskMode {
            get {
                return ((ViewModel.ShellViewModel)model).IsOnTaskMode;
            }            
        }

        private bool IsTaskListVisible {
            get {
                return ((ViewModel.ShellViewModel)model).IsTaskListVisible;
            }
            set {
                ((ViewModel.ShellViewModel)model).IsTaskListVisible = value;
            }
        }

       

        public MainWindow() {
            this.InitializeComponent();

          
            Closed += new System.EventHandler(MainWindow_Closed);
            taskPanel.MouseLeave += new MouseEventHandler(taskList_MouseLeave);

        }

        void taskList_MouseLeave(object sender, MouseEventArgs e) {
            if (this.OwnedWindows.Count > 0)
                return;
            if (IsOnTaskMode)
                WindowState = System.Windows.WindowState.Minimized;
            else
                IsTaskListVisible = false;
        }

    
        
        void MainWindow_Closed(object sender, System.EventArgs e) {
            Application.Current.Shutdown();
        }

        public void SetTaskModeWindow() {
          
            this.Height = taskButton.Height + 320;
            this.BeginAnimation(TopProperty, null); // releases old animation
            this.Top = System.Windows.SystemParameters.WorkArea.Height - this.Height;
          
            mainWindow.Focus();
        }

        public void SetFullModeWindow() {                        
            this.Height = System.Windows.SystemParameters.WorkArea.Height;            
            System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation(0, new System.TimeSpan(0, 0, 0, 0, 0));
            this.BeginAnimation(TopProperty, ani);         
        }

        
        public new void Show() {
            WindowState = System.Windows.WindowState.Normal;
            
            SetMaximized();
            if(!this.IsVisible)
                base.Show();            
        }

        private void SetMaximized() {
            Left = System.Windows.SystemParameters.WorkArea.Left;                
            Width = System.Windows.SystemParameters.WorkArea.Width;
            Height = System.Windows.SystemParameters.WorkArea.Height;
            Top = 0;       
        }

        

        [Import(typeof(ViewModel.ShellViewModel))]
        public object Model
        {
            get
            {
                return this.model;
            }
            set
            {
                this.model = value;
                this.DataContext = value;                
            }
        }

    

        private void ShowTopMenu() {
            System.Windows.Media.Animation.ThicknessAnimation ani = new System.Windows.Media.Animation.ThicknessAnimation(
                  new Thickness(0, 0, 0, 0), new Duration(new System.TimeSpan(0, 0, 0, 0, 300)));
            mainGrid.BeginAnimation(MarginProperty, ani);      
        }

        private void HideTopMenu() {
            System.Windows.Media.Animation.ThicknessAnimation ani = new System.Windows.Media.Animation.ThicknessAnimation(
                    new Thickness(0, -40, 0, 0), new Duration(new System.TimeSpan(0, 0, 0, 0, 300)));
            mainGrid.BeginAnimation(MarginProperty, ani);
        }

        private void Border_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue)
                HideTopMenu();
            else
                ShowTopMenu();
        }

 


    
       

    }
}
