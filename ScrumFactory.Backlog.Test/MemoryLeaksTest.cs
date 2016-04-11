using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Windows.Input;
using ScrumFactory;
using ScrumFactory.Composition;
using System.ComponentModel;
using ScrumFactory.Test;
using Moq;

namespace ScrumFactory.Backlog.Test {

    [TestClass]
    public class MemoryLeaksTest {

        [TestMethod]
        public void ItemViewModel_Dispose_Ok() {

            // creates a Item ViewModel
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();            
            ViewModel.BacklogItemViewModel viewModel = new ViewModel.BacklogItemViewModel(
                null,
                null,
                aggregator,
                null,
                new Project() { Roles = new List<Role>() },
                new BacklogItem(), null);

            // creates the view
            FakeView view = new FakeView();
            view.Model = viewModel;
                        
            // clears VM reference
            WeakReference taskVMRef = new WeakReference(viewModel);
            viewModel.Dispose();
            viewModel = null;
            view.Model = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            Assert.IsNull(taskVMRef.Target, "ViewModel was not garbage collected. Check OnDispose method to see if all Commands and Events were unsubscribed");
    
        }

        [TestMethod]
        public void Disposing_Items_On_Project_Changed() {

            // creates a backlog list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();
            
            ViewModel.BacklogViewModel list = new ViewModel.BacklogViewModel(
                null,
                null,
                null,
                aggregator,
                null,
                null);

            // create a item and adds to the backlog list view model
            List<ViewModel.BacklogItemViewModel> items = new List<ViewModel.BacklogItemViewModel>();
            Mock<ViewModel.BacklogItemViewModel> itemVM = new Mock<ViewModel.BacklogItemViewModel>(
                null,
                null,
                aggregator,
                null,
                new Project() { Roles = new List<Role>() },
                new BacklogItem());
            items.Add(itemVM.Object);
            list.Items = items;

            // emulates a project change            
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, new Project());

            itemVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        [TestMethod]
        public void Disposing_Items_On_Filter_Changed() {

            Mock<ViewModel.BacklogItemViewModel> itemVM;
            ViewModel.BacklogViewModel list = PrepareListViewModel(out itemVM);

            // verify dispose on filter change
            list.StatusFilter = ScrumFactory.Backlog.ViewModel.BacklogStatusFilter.SELECTED_BACKLOG;
            itemVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        [TestMethod]
        public void Disposing_Items_On_Load() {

            Mock<ViewModel.BacklogItemViewModel> itemVM;
            ViewModel.BacklogViewModel list = PrepareListViewModel(out itemVM);

            // verify dispose on load command            
            list.LoadDataCommand.Execute(null);
            itemVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        [TestMethod]
        public void Disposing_Items_On_Delete() {

            Mock<ViewModel.BacklogItemViewModel> itemVM;
            ViewModel.BacklogViewModel list = PrepareListViewModel(out itemVM);

            // verify dispose on load command            
            list.DeleteBacklogItemCommand.Execute(itemVM.Object);
            itemVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        [TestMethod]
        public void SprintViewModel_Dispose_Ok() {

            // creates a Item ViewModel
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();
            ViewModel.SprintViewModel viewModel = new ViewModel.SprintViewModel(
                null,
                null,
                aggregator,
                new Sprint(),
                null, null, null);

            // creates the view
            FakeView view = new FakeView();
            view.Model = viewModel;

            // clears VM reference
            WeakReference sprintVMRef = new WeakReference(viewModel);
            viewModel.Dispose();
            viewModel = null;
            view.Model = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(sprintVMRef.Target, "ViewModel was not garbage collected. Check OnDispose method to see if all Commands and Events were unsubscribed");

        }

        [TestMethod]
        public void Disposing_Sprints_On_Project_Changed() {

            // creates a backlog list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();

            ViewModel.IterationPlanningViewModel list = new ViewModel.IterationPlanningViewModel(
                null,
                null,
                null,
                null,
                aggregator,
                null);

            // create a item and adds to the backlog list view model
            System.Collections.ObjectModel.ObservableCollection<ViewModel.SprintViewModel> sprints = new System.Collections.ObjectModel.ObservableCollection<ViewModel.SprintViewModel>();
            Mock<ViewModel.SprintViewModel> sprintVM = new Mock<ViewModel.SprintViewModel>(
                null,
                null,
                aggregator,
                new Sprint(),
                null);

            sprints.Add(sprintVM.Object);
            list.SprintPlans = sprints;

            // emulates a project change            
            Project p = new Project();
            p.Sprints = new List<Sprint>();
            p.Sprints.Add(new Sprint());
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);

            sprintVM.Verify(i => i.Dispose(), "Dispose was not called");

        
        }

        [TestMethod]
        public void Disposing_Sprints_On_Load() {

            Mock<ViewModel.SprintViewModel> sprintVM;
            ViewModel.IterationPlanningViewModel list = PrepareSprintListViewModel(out sprintVM);

            // verify dispose on load command            
            list.LoadDataCommand.Execute(null);
            sprintVM.Verify(i => i.Dispose(), "Dispose was not called");

        }


        //[TestMethod]
        //public void Disposing_Sprints_On_Delete() {

        //    Mock<ViewModel.SprintViewModel> sprintVM;
        //    ViewModel.IterationPlanningViewModel list = PrepareSprintListViewModel(out sprintVM);

        //    // verify dispose on load command            
        //    list.RemoveSprintCommand.Execute(sprintVM.Object);
        //    sprintVM.Verify(i => i.Dispose(), "Dispose was not called");

        //}



        private ViewModel.BacklogViewModel PrepareListViewModel(out Mock<ViewModel.BacklogItemViewModel> itemVM) {

            // creates a fake backlogservice
            Mock<Services.IBacklogService> backlogService = new Mock<Services.IBacklogService>();
            backlogService.Setup(b =>
                b.GetCurrentBacklog(It.IsAny<string>(), It.IsAny<short>(), DateTime.MinValue, DateTime.MinValue))
                .Returns(new List<BacklogItem>());

            // creates a immediate bg executor
            Mock<IBackgroundExecutor> executor = BackgroundExecutorMock.SetupExecutorForCollectionOf<BacklogItem>(null);
            BackgroundExecutorMock.SetupExecutorForAction(executor);

            // creates a backlog list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();

            ViewModel.BacklogViewModel list = new ViewModel.BacklogViewModel(
                backlogService.Object,
                null,
                executor.Object,
                aggregator,
                new Mock<IDialogService>().Object,
                null);

            // assign a project to the list (no items yet, so no dispose by now)            
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, new Project());

            // create a item and adds to the backlog list view model
            List<ViewModel.BacklogItemViewModel> items = new List<ViewModel.BacklogItemViewModel>();
            itemVM = new Mock<ViewModel.BacklogItemViewModel>(
                null,
                null,
                aggregator,
                null,
                new Project() { Roles = new List<Role>() },
                new BacklogItem());
            items.Add(itemVM.Object);
            list.Items = items;

            return list;

        }

        private ViewModel.IterationPlanningViewModel PrepareSprintListViewModel(out Mock<ViewModel.SprintViewModel> sprintVM) {

            // creates a fake backlogservice
            Mock<Services.IBacklogService> backlogService = new Mock<Services.IBacklogService>();
            backlogService.Setup(b =>
                b.GetCurrentBacklog(It.IsAny<string>(), It.IsAny<short>(), DateTime.MinValue, DateTime.MinValue))
                .Returns(new List<BacklogItem>());

            // creates a immediate bg executor
            Mock<IBackgroundExecutor> executor = BackgroundExecutorMock.SetupExecutorForCollectionOf<BacklogItem>(null);
            BackgroundExecutorMock.SetupExecutorForAction(executor);
    
            // creates a sprint list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();

            ViewModel.IterationPlanningViewModel list = new ViewModel.IterationPlanningViewModel(
                backlogService.Object,
                null,
                executor.Object,
                null,
                aggregator,
                null);

            

            // assign a project to the list (no items yet, so no dispose by now)            
            Project p = new Project();
            p.Sprints = new List<Sprint>();
            p.Sprints.Add(new Sprint() { SprintNumber = 1 });
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, p);

            // create a sprint and adds to the backlog list view model
            System.Collections.ObjectModel.ObservableCollection<ViewModel.SprintViewModel> sprints = new System.Collections.ObjectModel.ObservableCollection<ViewModel.SprintViewModel>();
            sprintVM = new Mock<ViewModel.SprintViewModel>(
                null,
                null,
                aggregator,
                p.Sprints[0],
                null);
            sprints.Add(sprintVM.Object);

            list.SprintPlans = sprints;

            return list;

        }

    }

}
