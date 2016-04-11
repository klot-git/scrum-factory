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

namespace ScrumFactory.Tasks.Test {

    [TestClass]
    public class MemoryLeaksTest {

        [TestMethod]
        public void TaskViewModel_Dispose_Ok() {

            // creates a Task ViewModel
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();
            ViewModel.TaskViewModel viewModel = new ViewModel.TaskViewModel(
                null,
                null,
                aggregator,
                null,
                null,                
                new Task(),
                null);

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
        public void Disposing_Tasks_On_Project_Changed() {

            // creates a Task list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();
            ViewModel.TasksListViewModel list = new ViewModel.TasksListViewModel(
                null,
                null,
                null,
                aggregator,
                null,
                null);

            // create a task and adds to the task list view model
            List<ViewModel.TaskViewModel> tasks = new List<ViewModel.TaskViewModel>();
            Mock<ViewModel.TaskViewModel> taskVM = new Mock<ViewModel.TaskViewModel>(
                null,
                null,
                aggregator,
                null,  
                null,
                new Task(),
                null);                            
            tasks.Add(taskVM.Object);                        
            list.Tasks = tasks;

            // emulates a project change
            Project project = new Project();
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, project);

            taskVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        [TestMethod]
        public void Disposing_Tasks_On_Load() {

            // creates a fake taskservice
            Mock<Services.ITasksService> tasksService = new Mock<Services.ITasksService>();
            tasksService.Setup(t =>
                t.GetProjectTasks(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(new List<Task>());

            // creates a immediate bg executor
            Mock<IBackgroundExecutor> executor = BackgroundExecutorMock.SetupExecutorForCollectionOf<Task>(null);
            
            // creates a Task list
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();            
            ViewModel.TasksListViewModel list = new ViewModel.TasksListViewModel(
                tasksService.Object,
                null,
                executor.Object,
                aggregator,
                null,
                null);                        
            
            // assign a project to the list (no tasks yet, so no dispose by now)            
            aggregator.Publish<Project>(ScrumFactoryEvent.ViewProjectDetails, new Project());

            // create a task and adds to the task list view model
            List<ViewModel.TaskViewModel> tasks = new List<ViewModel.TaskViewModel>();
            Mock<ViewModel.TaskViewModel> taskVM = new Mock<ViewModel.TaskViewModel>(
                null,
                null,
                aggregator,
                new Mock<IDialogService>().Object,
                null,
                new Task(),
                null);

            tasks.Add(taskVM.Object);
            list.Tasks = tasks;

            // emulates two load command
            list.OnLoadCommand.Execute(null);            

            taskVM.Verify(i => i.Dispose(), "Dispose was not called");

        }

        
    }

}
