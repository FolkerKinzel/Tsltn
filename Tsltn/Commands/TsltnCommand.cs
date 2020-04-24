using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnCommand
    {
        private static readonly RoutedUICommand _translate = new RoutedUICommand(Res.Translate, "Translate", typeof(TsltnCommand));

        


        public static RoutedUICommand Translate
        {
            get { return _translate; }
        }
    }
}
