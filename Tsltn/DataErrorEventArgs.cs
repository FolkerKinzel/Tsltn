using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;

namespace Tsltn
{
    public class DataErrorEventArgs : EventArgs
    {
        public DataErrorEventArgs(IEnumerable<DataError> errors)
        {
            this.Errors = errors;
        }
        public IEnumerable<DataError> Errors { get; }
    }
}
