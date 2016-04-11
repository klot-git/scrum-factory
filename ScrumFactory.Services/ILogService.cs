using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Services {
    public interface ILogService {
        void LogError(Exception ex);
        void LogText(String text);
    }
}
