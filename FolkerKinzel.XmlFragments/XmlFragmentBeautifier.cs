using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FolkerKinzel.XmlFragments
{
    public static class XmlFragmentBeautifier
    {
        private const string NonBreakingSpace = "&#160;";
        private const RegexOptions SINGLE_LINE = RegexOptions.Singleline | RegexOptions.Compiled;
        private const int MATCH_TIMEOUT = 500;

        private static readonly Regex[] _blockTags = new Regex[]
        {
            new Regex(@"<\s*\/?\s*inheritdoc.*?>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT)),
            new Regex(@"<\s*\/?\s*para\s*>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT)),
            new Regex(@"<\s*\/?\s*list.*?>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT)),
            new Regex(@"<\s*\/?\s*item\s*?>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT)),
            new Regex(@"<\s*\/?\s*note.*?>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT)),
            new Regex(@"<\s*\/?\s*code.*?>", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT))
        };
        
        private static readonly Regex _singleWhiteSpace =
            new Regex(@"\s", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT));

        private static readonly Regex _multiWhiteSpace =
            new Regex(@"\s+", SINGLE_LINE, TimeSpan.FromMilliseconds(MATCH_TIMEOUT));




        public static string Beautify(string s)
        {
            if(s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            s = s.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal);
            s = _singleWhiteSpace.Replace(s, " ");
            s = _multiWhiteSpace.Replace(s, " ");

            var allBlockTags = new List<Match>();

            foreach (Regex blockTag in _blockTags)
            {
                MatchCollection? matches = blockTag.Matches(s);
                allBlockTags.AddRange(matches);
            }

            if (allBlockTags.Count == 0)
            {
                return s.Trim();
            }
            else
            {
                allBlockTags.Sort((m1, m2) => m1.Index.CompareTo(m2.Index));

                var sb = new StringBuilder(s.Length + s.Length / 3);
                int currentIndex = 0;

                foreach (Match match in allBlockTags)
                {
                    currentIndex = GetSpanStart(s, currentIndex, match.Index);
                    int length = GetSpanLength(s, currentIndex, match.Index - currentIndex);

                    if(length != 0)
                    {
                        sb.Append(s.AsSpan(currentIndex, length));
                        sb.Append(Environment.NewLine);
                    }
                    
                    sb.Append(s.AsSpan(match.Index, match.Length));
                    sb.Append(Environment.NewLine);
                    currentIndex = match.Index + match.Length;
                }

                sb.Append(s.AsSpan(GetSpanStart(s, currentIndex, s.Length), GetSpanLength(s, currentIndex, s.Length - currentIndex)));
                return sb.ToString();
            }
        }


        private static int GetSpanStart(string s, int startIndex, int afterEndIndex)
        {
            while (startIndex < afterEndIndex && s[startIndex].Equals(' '))
            {
                startIndex++;
            };

            return startIndex;
        }


        private static int GetSpanLength(string s, int currentIndex, int length)
        {
            for (int i = currentIndex + length - 1; i >= currentIndex; i--)
            {
                if(s[i] == ' ')
                {
                    --length;
                }
                else
                {
                    return length;
                }
            }

            return length;
        }
    }
}
