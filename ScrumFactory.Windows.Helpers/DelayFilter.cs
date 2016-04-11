using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Windows.Helpers {

    public class DelayAction {

        public delegate void ActionDelegate();
        private ActionDelegate actionMethod;

        private System.Windows.Threading.DispatcherTimer actionTimer = new System.Windows.Threading.DispatcherTimer();

        private bool isRunning = false;

        private bool stopAfterOne = true;

        public DelayAction(int milliseconds, ActionDelegate actionMethod) {
            this.actionMethod = actionMethod;
            actionTimer.Interval = new TimeSpan(0, 0, 0, 0, milliseconds);
            actionTimer.Tick += new EventHandler(actionTimer_Tick);
        }

        public DelayAction(int milliseconds, ActionDelegate actionMethod, bool stopAfterOne) {
            this.actionMethod = actionMethod;
            actionTimer.Interval = new TimeSpan(0, 0, 0, 0, milliseconds);
            actionTimer.Tick += new EventHandler(actionTimer_Tick);
            this.stopAfterOne = stopAfterOne;
        }

        public bool IRunning {
            get {
                return isRunning;
            }
        }


        public void StartAction() {                    
            actionTimer.Stop();
            actionTimer.Start();
            isRunning = true;
        }

        public void Stop() {            
            actionTimer.Stop();
            isRunning = false;
        }

        private void actionTimer_Tick(object sender, EventArgs e) {
            actionMethod();
            if(stopAfterOne)
                Stop();
        }
    }

}
