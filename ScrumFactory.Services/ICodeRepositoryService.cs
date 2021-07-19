using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services {
    public interface ICodeRepositoryService {

        string RepositoryTypeName { get; }
        string RepositoryImage { get; }
        string RepositoryRoot { get; }
        bool LoadSettings(out string errorMessage);
        ICollection<string> LoadCodeReps(Project project);
        string CreateRepository(string repositoryName, out string errorMessage);
    }
}
