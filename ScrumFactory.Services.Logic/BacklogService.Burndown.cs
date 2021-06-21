using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;


namespace ScrumFactory.Services.Logic {

    public partial class BacklogService : IBacklogService {

        /// <summary>
        /// Gets the amount of hours left to finish all backlog items day by day.
        /// </summary>
        /// <param name="projectUId">The project unique identifier</param>
        /// <returns>A par collection of day and the amount of hours</returns>
        [WebGet(UriTemplate = "BurndownHoursByDay/{projectUId}?planning={planning}", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<BurndownLeftHoursByDay> GetBurndownHoursByDay(string projectUId, string planning) {

            authorizationService.VerifyRequestAuthorizationToken();
            Project project = projectsService.GetProject(projectUId);
            
            if(string.IsNullOrEmpty(planning))
                return GetBurndownHoursByDay(project, project.CurrentPlanningNumber);

            if(planning=="proposal")
                return GetBurndownHoursByDay(project, 0);

            if(planning=="first")
                return GetBurndownHoursByDay(project, 1);

            int p = 0;
            int.TryParse(planning, out p);
            return GetBurndownHoursByDay(project, p);
        }

        private ICollection<BurndownLeftHoursByDay> GetBurndownHoursByDay(Project project, int planning) {

            List<BurndownLeftHoursByDay> allDaysHours = new List<BurndownLeftHoursByDay>();

            // gets the project            
            if (project == null || project.Sprints == null || project.Sprints.Count == 0)
                return allDaysHours;

            // gets the items
            List<BacklogItem> items = GetBacklog(project.ProjectUId, null).ToList();
            if (items == null)
                return allDaysHours;

            // remove itens that are currently out of the backlog
            items = items.Where(i => i.SprintNumber >= 0).ToList();

            // gets the left hours            
            BurndownLeftHoursByDay[] leftHours = (BurndownLeftHoursByDay[])GetHoursByDay_LEFT_HOURS(project, items);
            BurndownLeftHoursByDay[] aheadHours = (BurndownLeftHoursByDay[])GetHoursByDay_LEFT_HOURS_AHEAD(leftHours, project);
            BurndownLeftHoursByDay[] planningHours = null;

            planningHours = (BurndownLeftHoursByDay[])GetPlanningHours(planning, project, items);
            
            allDaysHours.AddRange(leftHours);
            allDaysHours.AddRange(aheadHours);
            allDaysHours.AddRange(planningHours);

            if (allDaysHours.Count == 0) {
                allDaysHours.Add(
                    new BurndownLeftHoursByDay() { Date = project.FirstSprint.StartDate, TotalHours = 0, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS_AHEAD });
                allDaysHours.Add(
                    new BurndownLeftHoursByDay() { Date = project.LastSprint.EndDate, TotalHours = 0, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS_AHEAD, IsLastSprint = true });
            }

            foreach (var h in allDaysHours)
            {
                h.Date = h.Date.Date; // removes the time part
            }


            return allDaysHours;



        }

        private ICollection<BurndownLeftHoursByDay> GetPlanningHours(int planningX, Project project, List<BacklogItem> items) {
            List<BurndownLeftHoursByDay> dayCurrentHours = new List<BurndownLeftHoursByDay>();

            foreach (Sprint s in project.Sprints.OrderBy(s => s.SprintNumber)) {
                //int planning = s.SprintNumber;
                //if (planning > planningX)
                //    planning = planningX;

                // compare always with first plan
                int planning = 1;

                // if project still at proposal, compare with proposal plan
                if(project.CurrentPlanningNumber==0) 
                    planning = 0;

                // use the parameter plan
                if (s.SprintNumber >= planningX)
                    planning = planningX;

                decimal hours = GetTotalPlannedHoursForSprint(planning, s.SprintNumber, items);
                dayCurrentHours.Add(new BurndownLeftHoursByDay() { Date = s.StartDate, TotalHours = hours, SprintNumber = s.SprintNumber, LeftHoursMetric = LeftHoursMetrics.PLANNING });

                if (DateTime.Today >= s.StartDate && DateTime.Today <= s.EndDate) {
                    decimal todayHours = GetHoursLeftForDay(planningX, s, items, DateTime.Today);
                    dayCurrentHours.Add(new BurndownLeftHoursByDay() { Date = DateTime.Today, TotalHours = todayHours, SprintNumber = s.SprintNumber, LeftHoursMetric = LeftHoursMetrics.PLANNING });
                }
            }

            dayCurrentHours.Add(new BurndownLeftHoursByDay() { Date = project.LastSprint.EndDate, TotalHours = 0, SprintNumber = project.LastSprint.SprintNumber, LeftHoursMetric = LeftHoursMetrics.PLANNING, IsLastSprint = true });

            foreach (var h in dayCurrentHours)
            {
                h.Date = h.Date.Date; // removes the time part
            }


            return dayCurrentHours.ToArray();

        }

        private decimal GetTotalPlannedHoursForSprint(int planningNumber, int sprintNumber, ICollection<BacklogItem> items) {
            decimal h = 0;
            foreach (BacklogItem item in items) {
                List<PlannedHour> hours = item.GetPlannedHoursAtPlanning(planningNumber);
                if (hours != null) {
                    decimal? total = hours.Where(o => o.SprintNumber >= sprintNumber).Sum(o => o.Hours);
                    if (total != null)
                        h = h + (decimal)total;
                }
            }
            return h;
        }

        private ICollection<BurndownLeftHoursByDay> GetHoursByDay_LEFT_HOURS(Project project, List<BacklogItem> items) {

            List<BurndownLeftHoursByDay> dayLeftHours = new List<BurndownLeftHoursByDay>();

            decimal lastHours = GetProjectPlannedHoursAtPlanning(0, project.FirstSprint.StartDate, items);
            DateTime lastDay = project.FirstSprint.StartDate;
            Sprint stopSprint = project.SprintForDate(DateTime.Today);
            if (project.LastSprint.EndDate < DateTime.Today)
                stopSprint = project.LastSprint;
            if (stopSprint == null)
                return dayLeftHours.ToArray();
            foreach (Sprint s in project.Sprints.Where(s => s.SprintNumber <= stopSprint.SprintNumber).OrderBy(s => s.SprintNumber)) {

                decimal sprintInitialHours = lastHours;

                if (s.SprintNumber <= project.CurrentPlanningNumber) {
                    sprintInitialHours = GetProjectPlannedHoursAtPlanning(s.SprintNumber, s.StartDate, items);
                }


                // adds sprint start date
                dayLeftHours.Add(new BurndownLeftHoursByDay() { Date = s.StartDate, TotalHours = sprintInitialHours, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS });

                // substract the finished items
                lastHours = sprintInitialHours;
                var sprintItems = items.Where(i => i.FinishedAt != null && i.FinishedAt >= s.StartDate && i.FinishedAt <= s.EndDate).OrderBy(i => i.FinishedAt);
                foreach (BacklogItem item in sprintItems) {
                    lastDay = (DateTime)item.FinishedAt.Value.Date;
                    lastHours = lastHours - item.CurrentTotalHours;
                    if (lastHours < 0)
                        lastHours = 0;
                    BurndownLeftHoursByDay bd = dayLeftHours.FirstOrDefault(b => b.Date.Equals(lastDay));
                    if (bd != null)
                        bd.TotalHours = lastHours;
                    else
                        dayLeftHours.Add(new BurndownLeftHoursByDay() { Date = lastDay, TotalHours = lastHours, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS });
                }
            }

            // adds today
            if (DateTime.Today >= project.FirstSprint.StartDate && DateTime.Today <= project.LastSprint.EndDate)
                if (dayLeftHours.SingleOrDefault(b => b.Date.Equals(DateTime.Today)) == null)
                    dayLeftHours.Add(new BurndownLeftHoursByDay() { Date = DateTime.Today, TotalHours = lastHours, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS });

            foreach (var h in dayLeftHours)
            {
                h.Date = h.Date.Date; // removes the time part
            }

            return dayLeftHours.ToArray();

        }

        private ICollection<BurndownLeftHoursByDay> GetHoursByDay_LEFT_HOURS_AHEAD(ICollection<BurndownLeftHoursByDay> leftHours, Project project) {

            if (DateTime.Today > project.LastSprint.EndDate || DateTime.Today < project.FirstSprint.StartDate)
                return new BurndownLeftHoursByDay[0];

            if(leftHours.Count==0)
                return new BurndownLeftHoursByDay[0];
            
            BurndownLeftHoursByDay last = leftHours.OrderBy(h => h.Date).Last();

            BurndownLeftHoursByDay[] hoursAhead = new BurndownLeftHoursByDay[2];
            hoursAhead[0] = new BurndownLeftHoursByDay() { Date = last.Date.Date, TotalHours = last.TotalHours, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS_AHEAD };
            hoursAhead[1] = new BurndownLeftHoursByDay() { Date = project.LastSprint.EndDate.Date, TotalHours = last.TotalHours, LeftHoursMetric = LeftHoursMetrics.LEFT_HOURS_AHEAD, IsLastSprint = true };

            foreach (var h in hoursAhead)
            {
                h.Date = h.Date.Date; // removes the time part
            }

            return hoursAhead;

        }

       
        
        private decimal GetHoursLeftForDay(int planningNumber, Sprint sprint, ICollection<BacklogItem> items, DateTime day) {
            if (day < sprint.StartDate.Date || day > sprint.EndDate.Date)
                return 0;
            decimal hours0 = GetTotalPlannedHoursForSprint(planningNumber, sprint.SprintNumber, items);
            decimal hours1 = GetTotalPlannedHoursForSprint(planningNumber, sprint.SprintNumber + 1, items);
            int days = sprint.EndDate.Date.Subtract(sprint.StartDate.Date).Days;
            if (days == 0)
                return 0;
            decimal ratio = (hours0 - hours1) / days;
            return hours0 - (ratio * day.Subtract(sprint.StartDate.Date).Days);
        }

        private decimal GetProjectPlannedHoursAtPlanning(int planningNumber, DateTime planningStartDate, ICollection<BacklogItem> items) {
            decimal h = 0;
            foreach (BacklogItem item in items.Where(i => i.FinishedAt == null || i.FinishedAt.Value.Date > planningStartDate.Date))
                h = h + item.GetTotalHoursAtPlanning(planningNumber);
            return h;
        }

        


    }
}
