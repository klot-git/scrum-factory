using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Transactions;
using System.Linq;
using System;

namespace ScrumFactory.Data.Sql {

    [Export(typeof(ICalendarRepository))]
    public class SqlCalendarRepository : ICalendarRepository {

        private string connectionString;

        [ImportingConstructor()]
        public SqlCalendarRepository([Import("ScrumFactoryEntitiesConnectionString")] string connectionString){
            this.connectionString = connectionString;
        }

        public ICollection<CalendarDay> GetHolidays(int fromYear) {
            using (var context = new ScrumFactoryEntities(connectionString)) {
                return context.CalendarDays.Where(d => d.Year == 0 || d.Year >= fromYear).ToArray();
            }
        }
    }
}
