using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GStringCompiler.Extension
{
    public static class ObjectExtension
    {
        public static bool IsNumeric(this object expression)
        {
            if (expression == null)
                return false;

            double number;
            return Double.TryParse(Convert.ToString(expression, CultureInfo.InvariantCulture), System.Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
        }
    }
}
