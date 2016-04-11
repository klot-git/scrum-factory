namespace ScrumFactory.Composition
{
    using System;

    public interface IBackgroundExecutor
    {
        void StartBackgroundTask<TResult>(Func<TResult> task, Action<TResult> callback);
        void StartBackgroundTask<TResult>(Func<TResult> task, Action<TResult> callback, Action<System.Exception> onError);
        
        
        void StartBackgroundTask(Action task, Action callback);
        void StartBackgroundTask(Action task, Action callback, Action<System.Exception> onError);
        
    }
}
