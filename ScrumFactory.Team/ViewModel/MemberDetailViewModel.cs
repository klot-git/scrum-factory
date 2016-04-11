using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team.ViewModel {

    [Export]
    public class MemberDetailViewModel : MemberDetailBase, INotifyPropertyChanged, IViewModel {

        
        private bool isLoading = false;

        [Import]
        private IDialogService dialogs { get; set; }


      
        [ImportingConstructor]
        public MemberDetailViewModel(
            [Import] IEventAggregator aggregator,
            [Import] ITasksService tasksService,
            [Import] IProjectsService projectsService,
            [Import] ITeamService teamService,
            [Import] IBackgroundExecutor executor,
            [Import] IAuthorizationService authorizator)
            : base(aggregator, tasksService, projectsService, teamService, executor, authorizator) {


                CloseWindowCommand = new DelegateCommand(() => {
                    Close();
                    dialogs.GoBackSelectedTopMenu();
                });
                
        }

        [Import(typeof(MemberDetail))]
        public IView View { get; set; }


        public bool CanActiveInative {
            get {
                if (authorizator == null || authorizator.SignedMemberProfile == null)
                    return false;
                return authorizator.SignedMemberProfile.IsFactoryOwner;
            }
        }

        public bool IsDisabled {
            get {
                return !MemberProfile.IsActive;
            }
            set {
                if (MemberProfile == null)
                    return;

                if (isLoading) {
                    MemberProfile.IsActive = !value;
                    OnPropertyChanged("IsDisabled");
                    return;
                }
                else {
                    executor.StartBackgroundTask(
                        () => { teamService.UpdateMemberIsActive(MemberProfile.MemberUId, !value); },
                        () => {
                            MemberProfile.IsActive = !value;
                            OnPropertyChanged("IsDisabled");
                        });
                }
            }
        }

        
        public void Show(IChildWindow parentViewModel, MemberProfile member) {
            
            isLoading = true;
            
            MemberProfile = member;
            OnPropertyChanged("CanActiveInative");
            IsDisabled = !MemberProfile.IsActive;
            isLoading = false;

            LoadMemberTasks();
            LoadMemberEngagedProjects();
            LoadMemberWorkingWithMembers();
            LoadMemberPerfomance();



            Show(parentViewModel);
        }



        

        public ICommand CloseWindowCommand { get; set; }
       
        

    }
}
