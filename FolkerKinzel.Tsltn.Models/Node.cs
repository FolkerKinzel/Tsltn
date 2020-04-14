using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public class Node
    {
        private readonly XElement _xElement;


        internal Node(XElement el)
        {
            this._xElement = el;

            this.Translation = Document.GetManualTranslation(_xElement);

            if(Translation != null)
            {
                IsManualTranslation = true;
            }
            else
            {
                Translation = Document.GetAutoTranslation(_xElement);
            }

            this.NodePath = Utility.GetNodePath(_xElement);
        }

        public string? Translation { get; set; }

        public bool IsManualTranslation { get; set; }

        public string NodePath { get; }


        public Node? GetNextNode()
        {
            var el = Document.GetNextNode(_xElement);

            if(el is null)
            {
                return null;
            }

            return new Node(el);
        }


        public Node? GetPreviousNode()
        {
            var el = Document.GetPreviousNode(_xElement);

            if (el is null)
            {
                return null;
            }

            return new Node(el);
        }


        public Node? GetNextUntranslated()
        {
            var el = Document.GetNextUntranslated(_xElement);

            if (el is null)
            {
                return null;
            }

            return new Node(el);
        }

    }
}
