using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows.Threading;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System.IO;
using System.IO.IsolatedStorage;

namespace ScrumFactory.Windows {
    

    [Export(typeof(IBackgroundExecutor))]
    public class BackgroundExecutor : IBackgroundExecutor
    {
        private Dispatcher dispatcher;


        [Import]
        private Services.ILogService log { get; set; }

        public BackgroundExecutor() {
            // This is actually a little tricky, since we know the BackgroundExecutor is created in the UI thread
            // during the Application class bootstrap
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void StartBackgroundTask<TResult>(Func<TResult> task, Action<TResult> callback) {
            StartBackgroundTask<TResult>(task, callback, null);
        }
        
        public void StartBackgroundTask<TResult>(Func<TResult> task, Action<TResult> callback, Action<System.Exception> onError) {
            TResult result;

            System.Console.Out.WriteLine(DateTime.Now +  "   ********************* action: " + task.Method.Name);

            new Thread(() => {
                try {
                    result = task();
                    dispatcher.Invoke(new Action<TResult>(r => { callback(r); }), result);
                }                                
                catch (Exception ex) {

                    // if is a scrum factory message, traslate it
                    if (ex is ScrumFactory.Exceptions.ScrumFactoryException)
                        ((ScrumFactory.Exceptions.ScrumFactoryException)ex).LocalizeException(task.Target.GetType());

                    HandleException(ex, onError);
                }

            }).Start();
        }

        public void StartBackgroundTask(Action task, Action callback) {
            StartBackgroundTask(task, callback, null);
        }

        
        public void StartBackgroundTask(Action task, Action callback, Action<System.Exception> onError) {

            System.Console.Out.WriteLine("********************* action: " + task.Method.Name);

            new Thread(() => {
                try {                    
                    task();
                    dispatcher.Invoke(callback, DispatcherPriority.Normal);
                } catch (Exception ex) {

                    // if is a scrum factory message, traslate it
                    if (ex is ScrumFactory.Exceptions.ScrumFactoryException)
                        ((ScrumFactory.Exceptions.ScrumFactoryException)ex).LocalizeException(task.Target.GetType());

                    HandleException(ex, onError);
                }                
                }).Start();
        }

        private void HandleException(Exception ex, Action<System.Exception> onError) {
            Exception exceptionToThrow = ex;

            log.LogError(ex);

            // is a netowrk error, users a ScrumFactory Newtwork exception
            if (ex is System.UriFormatException ||
                ex is System.Net.WebException ||
                ex is System.Net.Http.HttpRequestException) {                    
                    exceptionToThrow = new ScrumFactory.Exceptions.NetworkException();                                    
            }

            // PORQUE???

            // if was defined a custom error function
            //if (onError != null) {
            //    dispatcher.Invoke(onError, exceptionToThrow);
            //    return;
            //} else
            //    dispatcher.Invoke(new Action(() => { throw exceptionToThrow; }));
        }

               
    }
}
