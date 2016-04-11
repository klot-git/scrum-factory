using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using System.Windows.Input;
using System;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Team.ViewModel
{
    [Export]
    public class HowToUseViewModel : IViewModel
    {

       [Import(typeof(HowToUse))]
       public IView View { get; set; }

    }
}
