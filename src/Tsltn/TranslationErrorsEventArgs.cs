using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;

namespace Tsltn
{
    public class TranslationErrorsEventArgs : EventArgs
    {
        public TranslationErrorsEventArgs(IEnumerable<DataError> errors)
        {
            this.Errors = errors;
        }

        public IEnumerable<DataError> Errors { get; }
    }
}
