using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshmup.HelperClasses
{
    public class Dialog : IEnumerator<DialogElement>, IEnumerable<DialogElement>
    {
        object IEnumerator.Current => Current;
        public DialogElement Current {  get => _current; }

        private int position;

        private DialogElement[] elements;

        public bool MoveNext()
        {
            if (elements.Length == 0) return false;
            _current = elements[++position];
            return position < elements.Length;
        }
        public void Reset()
        {
            position = -1;
        }
        private DialogElement _current;
        public IEnumerator<DialogElement> GetEnumerator()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Dialog(DialogElement[] elements)
        {
            position = -1;
            this.elements = elements;
            _current = new(ElementType.TextLine, "", "");
        }

        public static Dialog FromArray(DialogElement[] elements)
        {
            return new Dialog(elements);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public static class DialogParser
    {
        private static string[] RemoveEmpty(string[] inp)
        {
            List<string> res = new();
            foreach (string s in inp) if (!string.IsNullOrEmpty(s)) res.Add(s);
            return res.ToArray();
        }

        public static Dialog Parse(string dlg)
        {
            List<DialogElement> dialogElements = new();
            string currentAuthor = "";

            string[] lines = dlg.Split("\n");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#")) continue;
                if (lines[i].StartsWith("["))
                {
                    currentAuthor = lines[i].Substring(1, lines[i].IndexOf("]") - 1);
                    continue;
                }
                else if (currentAuthor == "") throw new Exception("No Author was specified before declaring a Dialog Line");
                if (lines[i].StartsWith("!"))
                    dialogElements.Add(new DialogElement(ElementType.BigTextLine, lines[i].Substring(1).Trim(), currentAuthor));
                else if (lines[i].StartsWith("§"))
                    dialogElements.Add(new DialogElement(ElementType.HardcodeEvent, lines[i].Substring(1).Trim(), currentAuthor));
                else if (lines[i].StartsWith(">"))
                {
                    List<string> choices = new();
                    bool start = false;
                    for (int ii = i; true; ii++)
                    {
                        if (!start && lines[ii].IndexOf("{") >= 0) start = true;
                        if (!start) continue;
                        choices.AddRange(RemoveEmpty(lines[ii].Substring(Math.Min(0, lines[ii].IndexOf("{"))).Split('{', ',', '\n', '}')));
                        if (lines[ii].IndexOf("}") >= 0) {
                            i = ii + 1;
                            break; 
                        }
                    }
                    dialogElements.Add(new DialogElement(lines[i].Substring(1, (lines[i].IndexOf("{") >= 0 ? lines[i].IndexOf("{") : lines[i].Length) - 1).Trim(), currentAuthor, choices.ToArray()));
                }
                else
                    dialogElements.Add(new DialogElement(ElementType.TextLine, lines[i], currentAuthor));
            }

            return Dialog.FromArray(dialogElements.ToArray());
        }

        public static string Stringify(Dialog dlg)
        {
            string res = "";
            string currentAuthor = "";
            foreach (DialogElement de in dlg)
            {
                if (de.Author != currentAuthor)
                {
                    res += $"[{currentAuthor}]\n";
                    currentAuthor = de.Author;
                }
                res += de.Stringify();
            }
            return res;
        }
    }

    public class DialogElement
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public ElementType Type {  get; set; }
        public string[]? Choices { get; set; }

        public DialogElement(ElementType type, string content, string author)
        {
            if (type == ElementType.Prompt) throw new ArgumentException("Prompt Type has to use the Constructor with Choices");
            Type = type;
            Content = content;
            Author = author;
            Choices = null;
        }
        public DialogElement(string content, string author, string[] choices)
        {
            Type = ElementType.Prompt;
            Content = content;
            Author = author;
            Choices = choices;
        }

        public string Stringify()
        {
            if (Type != ElementType.Prompt)
                return $"{new string[] { "", "!", ">", "§" }[(int)Type]}{Content}\n";
            else
                return $"{new string[] { "", "!", ">", "§" }[(int)Type]}{Content} {{ {string.Join(", ", Choices ?? Array.Empty<string>())} }}\n";
        }
    }

    public enum ElementType
    {
        TextLine = 0,
        BigTextLine = 1,
        Prompt = 2,
        HardcodeEvent = 3
    }
}
