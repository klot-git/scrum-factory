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

namespace ScrumFactory.Team.Test {

    [TestClass]
    public class MemoryLeaksTest {

        [TestMethod]
        public void MembershipModel_Dispose_Ok() {

            // creates a Task ViewModel            
            ViewModel.ProjectMembershipViewModel viewModel = new ViewModel.ProjectMembershipViewModel(
                null,
                null,                
                null,                
                new ProjectMembership(),
                new MemberProfile());

            // creates the view
            FakeView view = new FakeView();
            view.Model = viewModel;

            //if (System.Windows.Application.Current == null) {
            //    ScrumFactory.Windows.App application = new ScrumFactory.Windows.App();
            //    application.InitializeComponent();
            //}

            //ProjectTeam view = new ProjectTeam();
            //view.Model = viewModel;
                        
            // clears VM reference
            WeakReference msVMRef = new WeakReference(viewModel);
            viewModel.Dispose();
            viewModel = null;
            view.Model = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            Assert.IsNull(msVMRef.Target, "ViewModel was not garbage collected. Check OnDispose method to see if all Commands and Events were unsubscribed");
    
        }

        [TestMethod]
        public void MemberModel_Dispose_Ok() {

            // creates a Task ViewModel            
            ViewModel.MemberViewModel viewModel = new ViewModel.MemberViewModel(
                new MemberProfile(), null, null);

            // creates the view
            FakeView view = new FakeView();
            view.Model = viewModel;

            //if (System.Windows.Application.Current == null) {
            //    ScrumFactory.Windows.App application = new ScrumFactory.Windows.App();
            //    application.InitializeComponent();
            //}

            //ProjectTeam view = new ProjectTeam();
            //view.Model = viewModel;

            // clears VM reference
            WeakReference msVMRef = new WeakReference(viewModel);
            viewModel.Dispose();
            viewModel = null;
            view.Model = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(msVMRef.Target, "ViewModel was not garbage collected. Check OnDispose method to see if all Commands and Events were unsubscribed");

        }
    }

}
