using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory {

    public enum PerformanceTrophies : short {
        TROPHY_BUG_KILLER,
        TROPHY_HARD_WORKER,
        TROPHY_IMPROVER,
        TROPHY_THE_FLASH,
        TROPHY_EXPERT
    }
   
    public class MemberPerformance {


        public string MemberUId { get; set; }
         
        public int HoursToNextLevel { get; set; }
         
        public int BugsResolved { get; set; }
         
        public int ImprovimentsDone { get; set; }
         
        public int TasksDone { get; set; }

        public int TasksDoneBeforePlanned { get; set; }
         
        public decimal TotalWorkedHours { get; set; }

        public decimal MonthWorkedHours { get; set; }

        public bool HasHardWorkerTrophy {
            get {
                //return true;
                return MonthWorkedHours > 160;
            }
        }

        public bool HasBugKillerTrophy {
            get {
                //return true;
                return BugsResolved > 3;
            }
        }

        public bool HasImproverTrophy {
            get {
                //return true;
                return ImprovimentsDone > 3;
            }
        }

        public bool HasExpertTrophy {
            get {
                //return true;
                return TotalWorkedHours > 2000;
            }
        }

        public bool HasTheFlashTrophy {
            get {
                //return true;
                return TasksDoneBeforePlanned > 3;
            }
        }

        public bool HasTrophy(PerformanceTrophies trophy) {
            return Trophies.Contains(trophy);
        }

        public ICollection<PerformanceTrophies> Trophies {
            get {
                List<PerformanceTrophies> thropies = new List<PerformanceTrophies>();
                if (HasExpertTrophy)
                    thropies.Add(PerformanceTrophies.TROPHY_EXPERT);
                if (HasBugKillerTrophy)
                    thropies.Add(PerformanceTrophies.TROPHY_BUG_KILLER);                
                if (HasHardWorkerTrophy)
                    thropies.Add(PerformanceTrophies.TROPHY_HARD_WORKER);
                if (HasImproverTrophy)
                    thropies.Add(PerformanceTrophies.TROPHY_IMPROVER);
                if (HasTheFlashTrophy)
                    thropies.Add(PerformanceTrophies.TROPHY_THE_FLASH);
                return thropies;
            }
        }



        //private int memberLevel = 0;
        //public int MemberLevel {
        //    get {
        //        if (memberLevel != 0)
        //            return memberLevel;

        //        int neededHoursToNextLevel = 20;
        //        int level = 1;

        //        while (neededHoursToNextLevel <= TotalWorkedHours) {
        //            level++;
        //            neededHoursToNextLevel += 20;

        //            if (level >= 55)
        //                neededHoursToNextLevel += 16;
        //        }

        //        int hoursNeededToCurrentLevel = neededHoursToNextLevel - 20;
        //        if (level >= 55)
        //            hoursNeededToCurrentLevel = hoursNeededToCurrentLevel - 16;

        //        neededHoursToNextLevel = neededHoursToNextLevel - hoursNeededToCurrentLevel;

        //        int currentHoursToNextLevel = (int)TotalWorkedHours - hoursNeededToCurrentLevel;

        //        memberLevel = level;
        //        HoursToNextLevel = neededHoursToNextLevel;

        //        return memberLevel;
        //    }
        //}

    }
}
