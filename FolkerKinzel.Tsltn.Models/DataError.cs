﻿using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public class DataError
    {
        public DataError(ErrorLevel level, string message, INode? node)
        {
            this.Level = level;
            this.Message = message;
            this.Node = node;
        }

        public ErrorLevel Level { get; }
        public string Message { get; }
        public INode? Node { get; set; }
    }
}
