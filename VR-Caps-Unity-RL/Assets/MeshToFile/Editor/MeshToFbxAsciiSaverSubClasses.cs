using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.MeshToFile;
using System.Text;
using System;
using System.Linq;

namespace Pinwheel.MeshToFile
{
    public partial class MeshToFbxAsciiSaver : IMeshSaver
    {
        internal class FbxProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public FbxProperty(string name, object value)
            {
                this.Name = name;
                this.Value = value.ToString();
            }

            public override string ToString()
            {
                return string.Format("{0}: {1}", Name, Value);
            }
        }

        internal class FbxNode
        {
            public string Name { get; set; }
            public string EventualProperties { get; set; }
            public List<FbxProperty> Properties { get; set; }
            public List<FbxNode> SubNodes { get; set; }

            public FbxNode(string name, object eventualProperties)
            {
                this.Name = name;
                EventualProperties = eventualProperties.ToString();

                Properties = new List<FbxProperty>();
                SubNodes = new List<FbxNode>();
            }

            public FbxNode AddProperty(string name, object value)
            {
                AddProperty(new FbxProperty(name, value));
                return this;
            }

            public FbxNode AddProperty(FbxProperty prop)
            {
                if (prop != null)
                    Properties.Add(prop);
                return this;
            }

            public FbxNode AddSubNode(FbxNode node)
            {
                if (node != null)
                    SubNodes.Add(node);
                return this;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Name).Append(": ").Append(EventualProperties).Append(Token.SPACE);
                sb.Append(Token.OPEN_CURLY).Append(Token.SPACE);

                for (int i = 0; i < Properties.Count; ++i)
                {
                    sb.Append(Properties[i].ToString()).Append(Token.EOL);
                }

                for (int i = 0; i < SubNodes.Count; ++i)
                {
                    sb.Append(SubNodes[i].ToString());
                }

                sb.Append(Token.CLOSE_CURLY).Append(Token.SPACE);
                return sb.ToString();
            }
        }

        internal struct Token
        {
            public const string EMPTY = "";
            public const string OPEN_CURLY = "{";
            public const string CLOSE_CURLY = "}";
            public const string OPEN_BRACKET = "[";
            public const string CLOSE_BRACKET = "]";
            public const string OPEN_PARENTHESE = "(";
            public const string CLOSE_PERENTHESE = ")";
            public const string COLON = ":";
            public const string SEMICOLON = ";";
            public const string DOT = ".";
            public const string COMMA = ",";
            public const string SPACE = " ";
            public const string LT = "<";
            public const string GT = ">";
            public const string EOL = "\n";
            public const string TAB = "\t";
            public const string QUOTE = "\"";
            public const string AMP = "&";
        }

        internal struct FbxFormatter
        {
            internal enum OpenCurlyBracketStyle
            {
                NewLine, Inline
            }

            public OpenCurlyBracketStyle BracketStyle { get; set; }

            /// <summary>
            /// Format the generated code
            /// </summary>
            /// <param name="s">Represent the code to format</param>
            /// <returns>Formatted code, broken into lines</returns>
            public string[] Format(string baseContent)
            {
                List<string> lines = new List<string>();

                //firstly, process the '{' token
                //if bracket style is NewLine, we put the token into a separated line
                //if braket style is Inline, we keep the token in its current line, and put the content after that onto a new line
                //do the same for each token
                if (BracketStyle == OpenCurlyBracketStyle.NewLine)
                {
                    baseContent = baseContent.Replace(Token.OPEN_CURLY, Token.EOL + Token.OPEN_CURLY + Token.EOL);
                }
                else
                {
                    baseContent = baseContent.Replace(Token.OPEN_CURLY, Token.OPEN_CURLY + Token.EOL);
                }

                //also put each '}' token on a separated line
                baseContent = baseContent.Replace(Token.CLOSE_CURLY, Token.EOL + Token.CLOSE_CURLY + Token.EOL);

                //split the code by eol token, remove leading and trailing whitespace on each line, then remove empty line
                lines = baseContent.Split(new string[] { Token.EOL }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < lines.Count; ++i)
                {
                    lines[i] = lines[i].Trim();
                }
                lines.RemoveAll(l => string.IsNullOrEmpty(l));

                //then add indentation for each line
                int tabCount = 0;
                for (int i = 0; i < lines.Count; ++i)
                {
                    if (lines[i].Contains(Token.CLOSE_CURLY))
                    {
                        tabCount -= 1;
                    }
                    lines[i] = TabString(lines[i], tabCount);
                    if (lines[i].Contains(Token.OPEN_CURLY))
                    {
                        tabCount += 1;
                    }
                }

                //nice!
                return lines.ToArray();
            }

            private static string TabString(string s, int tabCount)
            {
                StringBuilder b = new StringBuilder();
                for (int i = 0; i < tabCount; ++i)
                {
                    b.Append("\t");
                }
                b.Append(s);
                return b.ToString();
            }
        }
    }
}
