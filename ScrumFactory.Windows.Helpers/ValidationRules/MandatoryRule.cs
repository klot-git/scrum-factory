using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Globalization;


namespace ScrumFactory.Windows.Helpers.ValidationRules {

    public class MandatoryRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            String s = value as String;
            if (String.IsNullOrWhiteSpace(s))
                return new ValidationResult(false, "The field is mandatory.");
            else
                return ValidationResult.ValidResult;
        }
    }
}
