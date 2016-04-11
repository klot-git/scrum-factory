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


namespace ScrumFactory.Risks.Test {

    [TestClass]
    public class MemoryLeaksTest {

        [TestMethod]
        public void RiskViewModel_Dispose_Ok() {

            // creates a Task ViewModel
            IEventAggregator aggregator = new ScrumFactory.Composition.EventAggregator();
            ViewModel.RiskViewModel viewModel = new ViewModel.RiskViewModel(null, null, new Risk());

            // creates the view
            FakeView view = new FakeView();
            view.Model = viewModel;

            // clears VM reference
            WeakReference riskVMRef = new WeakReference(viewModel);
            viewModel.Dispose();
            viewModel = null;
            view.Model = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(riskVMRef.Target, "ViewModel was not garbage collected. Check OnDispose method to see if all Commands and Events were unsubscribed");

        }


    
    }      
}
