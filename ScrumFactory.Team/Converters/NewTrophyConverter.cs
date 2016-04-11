using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace ScrumFactory.Team.Converters {
    
    [ValueConversion(typeof(int), typeof(bool))]
    public class NewTrophyConverter : IValueConverter {


        public static MemberProfile SignedMember { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if (SignedMember == null || value==null)
                return false;

            ScrumFactory.PerformanceTrophies trophy;
            if (!Enum.TryParse(value.ToString(), out trophy))
                return false;

            if (!SignedMember.Performance.HasTrophy(trophy))
                return false;

            switch (trophy) {
                
                case PerformanceTrophies.TROPHY_EXPERT:
                    return HasWonNow(trophy);
                    
                default:
                    return HasWonThisMonth(trophy);
                    

            }
        }

        private bool HasWonThisMonth(PerformanceTrophies trophy) {
            System.DateTime? lastWonDate = (DateTime?) Properties.Settings.Default[trophy.ToString() + "_DATE"];
            if (!lastWonDate.HasValue || (lastWonDate < System.DateTime.Today && lastWonDate.Value.Month != System.DateTime.Today.Month)) {
                Properties.Settings.Default[trophy.ToString() + "_DATE"] = DateTime.Now;
                Properties.Settings.Default.Save();
                return true;
            }
            return false;
        }

        private bool HasWonNow(PerformanceTrophies trophy) {
            System.DateTime? lastWonDate = (DateTime?)Properties.Settings.Default[trophy.ToString() + "_DATE"];
            if (!lastWonDate.HasValue) {
                Properties.Settings.Default[trophy.ToString() + "_DATE"] = DateTime.Now;
                Properties.Settings.Default.Save();
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
