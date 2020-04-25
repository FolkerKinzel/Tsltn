using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnCommand
    {
        private static readonly RoutedCommand _translate = new RoutedCommand("Translate", typeof(TsltnCommand));

        


        public static RoutedCommand Translate
        {
            get { return _translate; }
        }
    }
}
