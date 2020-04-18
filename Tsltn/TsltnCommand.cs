using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn
{
    public static class TsltnCommand
    {
        private static readonly RoutedUICommand _translate;

        static TsltnCommand()
        {
            _translate = new RoutedUICommand(Res.Translate, "Translate", typeof(TsltnCommand));
        }


        public static RoutedUICommand Translate
        {
            get { return _translate; }
        }
    }
}
