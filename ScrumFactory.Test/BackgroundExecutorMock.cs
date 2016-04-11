using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition;
using Moq;

namespace ScrumFactory.Test {
    public class BackgroundExecutorMock {

        public static Mock<IBackgroundExecutor> SetupExecutorForCollectionOf<T>(Mock<IBackgroundExecutor> executor) {
            if(executor==null)
                executor = new Mock<IBackgroundExecutor>();
            executor.Setup(e =>
                e.StartBackgroundTask<ICollection<T>>(
                    It.IsAny<Func<ICollection<T>>>(),
                    It.IsAny<Action<ICollection<T>>>()))
                    .Callback<Func<ICollection<T>>, Action<ICollection<T>>>((a, c) => {
                        var r = a();
                        c(r);
                    });
            return executor;
        }

        public static Mock<IBackgroundExecutor> SetupExecutorForAction(Mock<IBackgroundExecutor> executor) {
            if (executor == null)
                executor = new Mock<IBackgroundExecutor>();
            executor.Setup(e => 
                e.StartBackgroundTask(
                It.IsAny<Action>(),
                It.IsAny<Action>()))
                .Callback<Action, Action>((a, c) => {
                    a();
                    c();
                });
            return executor;
        }

        public static Mock<IBackgroundExecutor> SetupExecutorFor<T>(Mock<IBackgroundExecutor> executor) {
            if (executor == null)
                executor = new Mock<IBackgroundExecutor>();
            executor.Setup(e =>
                e.StartBackgroundTask<T>(
                    It.IsAny<Func<T>>(),
                    It.IsAny<Action<T>>()))
                    .Callback<Func<T>, Action<T>>((a, c) => {
                        var r = a();
                        c(r);
                    });
            return executor;
        }

    }
}
