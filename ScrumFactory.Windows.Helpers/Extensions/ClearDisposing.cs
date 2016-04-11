using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Windows.Helpers.Extensions {

    public static class ClearDisposing {

        public static void ClearAndDispose<T>(this ICollection<T> c) {
            if (c == null)
                return;
            foreach (T i in c) 
                if(i is IDisposable)
                    ((IDisposable)i).Dispose();
            if(!c.IsReadOnly)
                c.Clear();
        }
    }
}
