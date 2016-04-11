using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {
    public interface IUserTasksSelectorViewModel {

        void StartTrack(Task task);
        void StopTaskTrack();
        void ResetTaskTrack();
        TimeSpan TrackEllipsedTime { get; }
        string TrackingTaskUId { get; }
    }
}
