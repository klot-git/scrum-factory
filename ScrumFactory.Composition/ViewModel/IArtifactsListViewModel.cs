using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Composition.ViewModel {

    public interface IArtifactsListViewModel : IViewModel {
        
        void ChangeContext(ArtifactContexts context, String contextUId, Action<int> count = null);
        ScrumFactory.ArtifactContexts ListContext { get; }
        string ContextUId { get; }
        ICollection<Artifact> Artifacts { get; }
    }
}
