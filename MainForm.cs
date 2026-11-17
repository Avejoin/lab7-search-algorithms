using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Lab7Search
{
    /// <summary>Главная форма: пять вкладок — по одной на каждую задачу лабораторной работы.</summary>
    public class MainForm : Form
    {
        private const string DefaultArray =
            "2, 5, 8, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60";

        private readonly TabControl tabs = new TabControl();

        // элементы вкладки «Задача 1»
        private TextBox txtText;
        private TextBox txtPattern;
        private CheckBox chkIgnoreCase;
        private ListBox lstWords;
        private Label lblStatus1;

        public MainForm()
        {
            Text = "Лабораторная работа № 7 — Алгоритмы поиска";
            ClientSize = new Size(900, 700);
            MinimumSize = new Size(760, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            tabs.Dock = DockStyle.Fill;
            tabs.TabPages.Add(BuildTextTab());
            tabs.TabPages.Add(BuildArrayTab(
                "Задача 2",
                "Задача 2. Найти наименьший элемент в упорядоченном массиве A " +
                "(линейный, бинарный и индексно-последовательный поиск).",
                null, 0, (a, p) => ReportMin(a)));
            tabs.TabPages.Add(BuildArrayTab(
                "Задача 3",
                "Задача 3. Найти элементы упорядоченного массива A, которые больше заданного числа " +
                "(по условию — 30). Методы: линейный, бинарный, индексно-последовательный.",
                "Больше, чем:", 30, (a, p) => ReportGreater(a, p)));
            tabs.TabPages.Add(BuildArrayTab(
                "Задача 4",
                "Задача 4. Вывести все числа массива A, кратные 3 (3, 6, 9, ...), " +
                "тремя методами поиска.",
                null, 0, (a, p) => ReportMultiples(a, 3)));
            tabs.TabPages.Add(BuildArrayTab(
                "Задача 5",
                "Задача 5. Вывести все числа упорядоченного массива A, кратные 4 (4, 8, ...), " +
                "тремя методами поиска.",
                null, 0, (a, p) => ReportMultiples(a, 4)));

            Controls.Add(tabs);
        }

        // =====================================================================
        //  Задача 1. Поиск строки в тексте
        // =====================================================================
        private TabPage BuildTextTab()
        {
            var page = new TabPage("Задача 1");
            page.Padding = new Padding(12);

            var lblDesc = new Label
            {
                Text = "Задача 1. Введите текст и искомую строку. Программа выведет все слова, " +
                       "в которых встречается эта строка.",
                Left = 12, Top = 10, Width = 860, Height = 40
            };
            var lblText = new Label { Text = "Текст:", Left = 12, Top = 56, AutoSize = true };
            txtText = new TextBox
            {
                Left = 12, Top = 80, Width = 860, Height = 140,
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var lblPattern = new Label { Text = "Искомая строка:", Left = 12, Top = 232, AutoSize = true };
            txtPattern = new TextBox { Left = 150, Top = 228, Width = 280 };
            chkIgnoreCase = new CheckBox
            {
                Text = "Не учитывать регистр", Left = 450, Top = 230, Width = 200, Checked = true
            };
            var btnFind = new Button { Text = "Найти", Left = 670, Top = 226, Width = 100, Height = 32 };
            btnFind.Click += OnFindClick;

            lblStatus1 = new Label { Left = 12, Top = 268, Width = 860, Height = 24 };

            var lblList = new Label { Text = "Слова, содержащие искомую строку:", Left = 12, Top = 298, AutoSize = true };
            lstWords = new ListBox
            {
                Left = 12, Top = 322, Width = 860, Height = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            page.Controls.AddRange(new Control[]
            {
                lblDesc, lblText, txtText, lblPattern, txtPattern, chkIgnoreCase,
                btnFind, lblStatus1, lblList, lstWords
            });
            return page;
        }

        private void OnFindClick(object sender, EventArgs e)
        {
            lstWords.Items.Clear();
            string text = txtText.Text;
            string pattern = txtPattern.Text;

            // --- проверка входных данных ---
            if (string.IsNullOrWhiteSpace(text))
            {
                SetStatus(lblStatus1, "Ошибка: текст не введён.", false);
                return;
            }
            if (string.IsNullOrEmpty(pattern))
            {
                SetStatus(lblStatus1, "Ошибка: не задана искомая строка.", false);
                return;
            }
            if (pattern.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                SetStatus(lblStatus1,
                    "Ошибка: искомая строка не должна содержать пробелов (поиск ведётся внутри слов).", false);
                return;
            }
            if (TextSearch.SplitWords(text).Count == 0)
            {
                SetStatus(lblStatus1, "Ошибка: в тексте нет ни одного слова.", false);
                return;
            }

            // --- поиск ---
            List<string> words = TextSearch.FindWords(text, pattern, chkIgnoreCase.Checked);
            if (words.Count == 0)
            {
                SetStatus(lblStatus1, "Строка «" + pattern + "» в тексте не найдена.", false);
                return;
            }

            foreach (string w in words)
            {
                lstWords.Items.Add(w);
            }
            SetStatus(lblStatus1,
                "Строка «" + pattern + "» найдена. Количество слов с вхождением: " + words.Count + ".", true);
        }

        // =====================================================================
        //  Задачи 2–5. Общий конструктор вкладки с массивом
        // =====================================================================
        private TabPage BuildArrayTab(string title, string description, string limitCaption,
                                      int limitDefault, Func<int[], int, string> action)
        {
            var page = new TabPage(title);
            page.Padding = new Padding(12);

            var lblDesc = new Label { Text = description, Left = 12, Top = 10, Width = 860, Height = 40 };
            var lblArr = new Label
            {
                Text = "Массив A (по возрастанию, через запятую):", Left = 12, Top = 56, AutoSize = true
            };
            var txtArr = new TextBox
            {
                Text = DefaultArray, Left = 12, Top = 80, Width = 860,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            NumericUpDown numLimit = null;
            int nextTop = 116;
            if (limitCaption != null)
            {
                var lblLimit = new Label { Text = limitCaption, Left = 12, Top = nextTop + 4, AutoSize = true };
                numLimit = new NumericUpDown
                {
                    Left = 150, Top = nextTop, Width = 100,
                    Minimum = -1000000, Maximum = 1000000, Value = limitDefault
                };
                page.Controls.Add(lblLimit);
                page.Controls.Add(numLimit);
            }

            var btn = new Button { Text = "Выполнить", Left = 12, Top = nextTop + 36, Width = 120, Height = 32 };
            var lblStatus = new Label { Left = 150, Top = nextTop + 42, Width = 720, Height = 24 };
            var txtOut = new TextBox
            {
                Left = 12, Top = nextTop + 80, Width = 860, Height = 330,
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btn.Click += (s, e) =>
            {
                int[] arr;
                string error;
                if (!ArrayParser.TryParse(txtArr.Text, out arr, out error))
                {
                    txtOut.Text = string.Empty;
                    SetStatus(lblStatus, "Ошибка: " + error, false);
                    return;
                }
                int param = numLimit != null ? (int)numLimit.Value : 0;
                txtOut.Text = action(arr, param);
                SetStatus(lblStatus, "Поиск выполнен. Элементов в массиве: " + arr.Length + ".", true);
            };

            page.Controls.AddRange(new Control[] { lblDesc, lblArr, txtArr, btn, lblStatus, txtOut });
            return page;
        }

        // =====================================================================
        //  Формирование отчётов о результатах поиска
        // =====================================================================
        private static string ReportMin(int[] a)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Массив A: " + Join(a));
            sb.AppendLine();
            AppendMin(sb, "Линейный поиск", ArraySearch.LinearMin(a), a);
            AppendMin(sb, "Бинарный поиск", ArraySearch.BinaryMin(a), a);
            AppendMin(sb, "Индексно-последовательный поиск", ArraySearch.IndexSequentialMin(a), a);
            AppendIndexTable(sb, a);
            return sb.ToString();
        }

        private static string ReportGreater(int[] a, int limit)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Массив A: " + Join(a));
            sb.AppendLine("Условие: элементы больше " + limit);
            sb.AppendLine();
            AppendList(sb, "Линейный поиск", ArraySearch.LinearGreater(a, limit), a);
            AppendList(sb, "Бинарный поиск", ArraySearch.BinaryGreater(a, limit), a);
            AppendList(sb, "Индексно-последовательный поиск", ArraySearch.IndexSequentialGreater(a, limit), a);
            AppendIndexTable(sb, a);
            return sb.ToString();
        }

        private static string ReportMultiples(int[] a, int k)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Массив A: " + Join(a));
            sb.AppendLine("Условие: элементы, кратные " + k);
            sb.AppendLine();
            AppendList(sb, "Линейный поиск", ArraySearch.LinearMultiples(a, k), a);
            AppendList(sb, "Бинарный поиск", ArraySearch.BinaryMultiples(a, k), a);
            AppendList(sb, "Индексно-последовательный поиск", ArraySearch.IndexSequentialMultiples(a, k), a);
            AppendIndexTable(sb, a);
            return sb.ToString();
        }

        private static void AppendMin(StringBuilder sb, string method, SearchResult r, int[] a)
        {
            int idx = r.Indexes[0];
            sb.AppendLine(method + ":");
            sb.AppendLine("  наименьший элемент = " + a[idx] + " (индекс " + idx + ")");
            sb.AppendLine("  количество сравнений: " + r.Comparisons);
            sb.AppendLine();
        }

        private static void AppendList(StringBuilder sb, string method, SearchResult r, int[] a)
        {
            sb.AppendLine(method + ":");
            if (r.Indexes.Count == 0)
            {
                sb.AppendLine("  подходящих элементов не найдено");
            }
            else
            {
                var values = new List<string>();
                var indexes = new List<string>();
                foreach (int i in r.Indexes)
                {
                    values.Add(a[i].ToString());
                    indexes.Add(i.ToString());
                }
                sb.AppendLine("  найдено элементов: " + r.Indexes.Count);
                sb.AppendLine("  значения: " + string.Join(", ", values));
                sb.AppendLine("  индексы:  " + string.Join(", ", indexes));
            }
            sb.AppendLine("  количество сравнений: " + r.Comparisons);
            sb.AppendLine();
        }

        private static void AppendIndexTable(StringBuilder sb, int[] a)
        {
            List<int> table = ArraySearch.BuildIndexTable(a);
            var parts = new List<string>();
            foreach (int pos in table)
            {
                parts.Add(a[pos] + "→[" + pos + "]");
            }
            sb.AppendLine("Индексная таблица (шаг " + ArraySearch.GetStep(a.Length) + "): " +
                          string.Join("  ", parts));
        }

        private static string Join(int[] a)
        {
            var parts = new string[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                parts[i] = a[i].ToString();
            }
            return string.Join(", ", parts);
        }

        private static void SetStatus(Label label, string message, bool success)
        {
            label.Text = message;
            label.ForeColor = success ? Color.DarkGreen : Color.Firebrick;
        }
    }
}
