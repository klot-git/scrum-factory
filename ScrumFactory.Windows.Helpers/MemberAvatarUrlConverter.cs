using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ScrumFactory.Windows.Helpers;


namespace ScrumFactory.Windows.Helpers.Converters {

    [ValueConversion(typeof(string), typeof(string))]
    public class MemberAvatarUrlConverter : IValueConverter {

        public static int noCache;
        
        public static string ServerUrl { get; set; }

        public static void ResetCache() {
            Random random = new Random();
            noCache = random.Next(0, 100);
        }

        public static MemberProfile SignedMember { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            string memberUId = value as string;
            if (memberUId == null)
                memberUId = String.Empty;

            if(SignedMember!=null && SignedMember.MemberUId == memberUId)
                return MemberAvatarUrlConverter.ServerUrl + "/MemberImage.aspx?MemberUId=" + memberUId + "&noCache=" + noCache;

            return MemberAvatarUrlConverter.ServerUrl + "/MemberImage.aspx?MemberUId=" + memberUId;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
