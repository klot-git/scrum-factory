using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Extensions {

    public static class CloneExtension {

        public static T Clone<T>(this T source) {
            var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            using (var ms = new System.IO.MemoryStream()) {
                dcs.WriteObject(ms, source);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)dcs.ReadObject(ms);
            }
        }
    }
}
