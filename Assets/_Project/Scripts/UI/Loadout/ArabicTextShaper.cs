using System.Collections.Generic;
using System.Text;

namespace SeaLion.UI.Localization
{
    /// <summary>Shapes the project's compact Arabic UI vocabulary for legacy uGUI Text.</summary>
    public static class ArabicTextShaper
    {
        private readonly struct Forms
        {
            public readonly char Isolated;
            public readonly char Final;
            public readonly char Initial;
            public readonly char Medial;
            public readonly bool JoinsPrevious;
            public readonly bool JoinsNext;

            public Forms(char isolated, char final, char initial = '\0', char medial = '\0')
            {
                Isolated = isolated;
                Final = final;
                Initial = initial;
                Medial = medial;
                JoinsPrevious = final != '\0';
                JoinsNext = initial != '\0';
            }
        }

        private enum RunKind
        {
            Arabic,
            Separator,
            LeftToRight
        }

        private readonly struct Run
        {
            public readonly RunKind Kind;
            public readonly string Value;

            public Run(RunKind kind, string value)
            {
                Kind = kind;
                Value = value;
            }
        }

        private static readonly Dictionary<char, Forms> Glyphs = new Dictionary<char, Forms>
        {
            { 'ء', new Forms('\uFE80', '\0') },
            { 'آ', new Forms('\uFE81', '\uFE82') },
            { 'أ', new Forms('\uFE83', '\uFE84') },
            { 'ؤ', new Forms('\uFE85', '\uFE86') },
            { 'إ', new Forms('\uFE87', '\uFE88') },
            { 'ئ', new Forms('\uFE89', '\uFE8A', '\uFE8B', '\uFE8C') },
            { 'ا', new Forms('\uFE8D', '\uFE8E') },
            { 'ب', new Forms('\uFE8F', '\uFE90', '\uFE91', '\uFE92') },
            { 'ة', new Forms('\uFE93', '\uFE94') },
            { 'ت', new Forms('\uFE95', '\uFE96', '\uFE97', '\uFE98') },
            { 'ث', new Forms('\uFE99', '\uFE9A', '\uFE9B', '\uFE9C') },
            { 'ج', new Forms('\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0') },
            { 'ح', new Forms('\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4') },
            { 'خ', new Forms('\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8') },
            { 'د', new Forms('\uFEA9', '\uFEAA') },
            { 'ذ', new Forms('\uFEAB', '\uFEAC') },
            { 'ر', new Forms('\uFEAD', '\uFEAE') },
            { 'ز', new Forms('\uFEAF', '\uFEB0') },
            { 'س', new Forms('\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4') },
            { 'ش', new Forms('\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8') },
            { 'ص', new Forms('\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC') },
            { 'ض', new Forms('\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0') },
            { 'ط', new Forms('\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4') },
            { 'ظ', new Forms('\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8') },
            { 'ع', new Forms('\uFEC9', '\uFECA', '\uFECB', '\uFECC') },
            { 'غ', new Forms('\uFECD', '\uFECE', '\uFECF', '\uFED0') },
            { 'ف', new Forms('\uFED1', '\uFED2', '\uFED3', '\uFED4') },
            { 'ق', new Forms('\uFED5', '\uFED6', '\uFED7', '\uFED8') },
            { 'ك', new Forms('\uFED9', '\uFEDA', '\uFEDB', '\uFEDC') },
            { 'ل', new Forms('\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0') },
            { 'م', new Forms('\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4') },
            { 'ن', new Forms('\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8') },
            { 'ه', new Forms('\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC') },
            { 'و', new Forms('\uFEED', '\uFEEE') },
            { 'ى', new Forms('\uFEEF', '\uFEF0') },
            { 'ي', new Forms('\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4') }
        };

        public static string Shape(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            var lines = value.Replace("\r\n", "\n").Split('\n');
            for (var index = 0; index < lines.Length; index++)
                lines[index] = ToVisualOrder(ShapeLogical(lines[index]));
            return string.Join("\n", lines);
        }

        private static string ShapeLogical(string value)
        {
            var shaped = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                if (!Glyphs.TryGetValue(current, out var forms))
                {
                    shaped.Append(current);
                    continue;
                }

                var previous = FindPreviousBase(value, index - 1);
                var next = FindNextBase(value, index + 1);
                var joinsPrevious = previous >= 0 && Glyphs.TryGetValue(value[previous], out var previousForms)
                    && previousForms.JoinsNext && forms.JoinsPrevious;
                var joinsNext = next >= 0 && Glyphs.TryGetValue(value[next], out var nextForms)
                    && forms.JoinsNext && nextForms.JoinsPrevious;

                shaped.Append(joinsPrevious && joinsNext ? forms.Medial :
                    joinsPrevious ? forms.Final : joinsNext ? forms.Initial : forms.Isolated);
            }
            return shaped.ToString();
        }

        private static int FindPreviousBase(string value, int index)
        {
            for (; index >= 0; index--)
            {
                if (IsTransparent(value[index])) continue;
                return char.IsWhiteSpace(value[index]) ? -1 : index;
            }
            return -1;
        }

        private static int FindNextBase(string value, int index)
        {
            for (; index < value.Length; index++)
            {
                if (IsTransparent(value[index])) continue;
                return char.IsWhiteSpace(value[index]) ? -1 : index;
            }
            return -1;
        }

        private static bool IsTransparent(char value)
        {
            return value >= '\u064B' && value <= '\u065F' || value == '\u0670';
        }

        private static string ToVisualOrder(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            var runs = new List<Run>();
            var start = 0;
            var kind = GetKind(value[0]);
            for (var index = 1; index <= value.Length; index++)
            {
                var nextKind = index < value.Length ? GetKind(value[index]) : (RunKind)(-1);
                if (index < value.Length && nextKind == kind) continue;
                runs.Add(new Run(kind, value.Substring(start, index - start)));
                start = index;
                kind = nextKind;
            }

            var visual = new StringBuilder(value.Length);
            for (var index = runs.Count - 1; index >= 0; index--)
            {
                var run = runs[index];
                visual.Append(run.Kind == RunKind.Arabic ? Reverse(run.Value) : run.Value);
            }
            return visual.ToString();
        }

        private static RunKind GetKind(char value)
        {
            if (value >= '\uFE70' && value <= '\uFEFF' || IsTransparent(value)) return RunKind.Arabic;
            if (char.IsWhiteSpace(value)) return RunKind.Separator;
            return RunKind.LeftToRight;
        }

        private static string Reverse(string value)
        {
            var chars = value.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }
    }
}
