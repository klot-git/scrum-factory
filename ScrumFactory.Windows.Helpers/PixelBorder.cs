using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ScrumFactory.Windows.Helpers {
    public class PixelBorder : Border {

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            PresentationSource presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource == null)
                return;
            var dpiX = presentationSource.CompositionTarget.TransformFromDevice.M11;
            BorderThickness = new Thickness(BorderThickness.Left*dpiX, BorderThickness.Top*dpiX, BorderThickness.Right*dpiX, BorderThickness.Bottom*dpiX);
        }

    }

}
