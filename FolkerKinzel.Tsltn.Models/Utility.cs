using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public class Utility
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly List<string> _list = new List<string>();

        private Utility() { }

        public static Utility Instance { get; } = new Utility();


        public string GetNodePath(XText node)
        {
            FillStringBuilder(node);
            return _sb.ToString();
        }

        internal int GetNodeHash(XText node)
        {
            FillStringBuilder(node);
            return HashService.HashNodePath(_sb);
        }


        private void FillStringBuilder(XText node)
        {
            _list.Clear();
            _sb.Clear();

            XElement el = node.Parent;

            while (true)
            {
                if (el is null || el.Name.LocalName == "members")
                {
                    break;
                }

                string name = el.Name.LocalName;

                switch (name)
                {
                    case "member":
                        _list.Add(el.Attribute("name")?.Value ?? name);
                        break;
                    case "event":
                    case "exception":
                    case "permission":
                    case "seealso":
                    case "see":
                        _list.Add($"{name}={el.Attribute("cref")?.Value}");
                        break;
                    case "param":
                    case "typeparam":
                    case "paramref":
                    case "typeparamref":
                        _list.Add($"{name}={el.Attribute("name")?.Value}");
                        break;
                    case "revision":
                        _list.Add($"{name}={el.Attribute("version")?.Value}");
                        break;
                    case "conceptualLink":
                        _list.Add($"{name}={el.Attribute("target")?.Value}");
                        break;
                    default:
                        _list.Add(name);
                        break;
                }

                el = el.Parent;
            }


            for (int i = _list.Count - 1; i >= 0; i--)
            {
                _sb.Append(_list[i]);

                if (i != 0)
                {
                    _sb.Append('/');
                }
            }



        }

    }
}
