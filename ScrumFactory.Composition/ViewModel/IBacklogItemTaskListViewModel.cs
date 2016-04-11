using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {
    public interface IBacklogItemTaskListViewModel {
        BacklogItem Item { get; set; }
        ICollection<Task> Tasks { get; }
        decimal TotalEffectiveHours { get; }
        Action OnTaskChanged { get; set; }

        decimal GetTotalEffectiveHoursForSprint(Sprint sprint);
    }
}
