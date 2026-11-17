using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lab7Search
{
    /// <summary>Результат поиска: индексы найденных элементов и число сравнений.</summary>
    public class SearchResult
    {
        public List<int> Indexes = new List<int>();
        public int Comparisons;
    }

    /// <summary>Разбор и проверка массива, введённого пользователем.</summary>
    public static class ArrayParser
    {
        public static bool TryParse(string input, out int[] result, out string error)
        {
            result = null;
            error = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "массив не задан — введите числа через запятую.";
                return false;
            }

            string[] parts = input.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' },
                                         StringSplitOptions.RemoveEmptyEntries);
            var list = new List<int>();
            foreach (string p in parts)
            {
                int v;
                if (!int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                {
                    error = "«" + p + "» не является целым числом.";
                    return false;
                }
                if (Math.Abs((long)v) > 1000000)
                {
                    error = "число «" + p + "» выходит за допустимый диапазон ±1 000 000.";
                    return false;
                }
                list.Add(v);
            }

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] < list[i - 1])
                {
                    error = "массив не упорядочен по возрастанию: элемент " + list[i] +
                            " (позиция " + (i + 1) + ") меньше предыдущего " + list[i - 1] + ".";
                    return false;
                }
            }

            result = list.ToArray();
            return true;
        }
    }

    /// <summary>
    /// Задачи 2–5. Линейный, бинарный и индексно-последовательный поиск
    /// в упорядоченном (по возрастанию) массиве.
    /// </summary>
    public static class ArraySearch
    {
        // ---------- Индексная таблица (для индексно-последовательного поиска) ----------

        /// <summary>Шаг индексной таблицы: корень из длины массива (не менее 1).</summary>
        public static int GetStep(int n)
        {
            int s = (int)Math.Sqrt(n);
            return s < 1 ? 1 : s;
        }

        /// <summary>Индексная таблица: позиции каждого step-го элемента массива.</summary>
        public static List<int> BuildIndexTable(int[] a)
        {
            var table = new List<int>();
            int step = GetStep(a.Length);
            for (int i = 0; i < a.Length; i += step)
            {
                table.Add(i);
            }
            return table;
        }

        // ---------- Линейный поиск ----------

        public static SearchResult LinearMin(int[] a)
        {
            var r = new SearchResult();
            int min = 0;
            for (int i = 1; i < a.Length; i++)
            {
                r.Comparisons++;
                if (a[i] < a[min])
                {
                    min = i;
                }
            }
            r.Indexes.Add(min);
            return r;
        }

        public static SearchResult LinearGreater(int[] a, int limit)
        {
            var r = new SearchResult();
            for (int i = 0; i < a.Length; i++)
            {
                r.Comparisons++;
                if (a[i] > limit)
                {
                    r.Indexes.Add(i);
                }
            }
            return r;
        }

        public static SearchResult LinearMultiples(int[] a, int k)
        {
            var r = new SearchResult();
            for (int i = 0; i < a.Length; i++)
            {
                r.Comparisons++;
                if (a[i] % k == 0)
                {
                    r.Indexes.Add(i);
                }
            }
            return r;
        }

        // ---------- Бинарный поиск ----------

        /// <summary>
        /// Бинарный поиск границы: первый индекс, где a[i] &gt;= value (strict = false)
        /// или a[i] &gt; value (strict = true). Если такого нет — a.Length.
        /// </summary>
        private static int BinaryBound(int[] a, int value, bool strict, ref int cmp)
        {
            int lo = 0;
            int hi = a.Length;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                cmp++;
                bool goRight = strict ? a[mid] <= value : a[mid] < value;
                if (goRight)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            return lo;
        }

        public static SearchResult BinaryMin(int[] a)
        {
            var r = new SearchResult();
            int lo = 0;
            int hi = a.Length - 1;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                r.Comparisons++;
                if (a[mid] <= a[hi])
                {
                    hi = mid;      // минимум в левой половине (включая mid)
                }
                else
                {
                    lo = mid + 1;  // минимум в правой половине
                }
            }
            r.Indexes.Add(lo);
            return r;
        }

        public static SearchResult BinaryGreater(int[] a, int limit)
        {
            var r = new SearchResult();
            int start = BinaryBound(a, limit, true, ref r.Comparisons);
            for (int i = start; i < a.Length; i++)
            {
                r.Indexes.Add(i);
            }
            return r;
        }

        public static SearchResult BinaryMultiples(int[] a, int k)
        {
            var r = new SearchResult();
            int min = a[0];
            int max = a[a.Length - 1];
            int first = (min / k) * k;
            if (first < min)
            {
                first += k;
            }
            for (int v = first; v <= max; v += k)
            {
                int idx = BinaryBound(a, v, false, ref r.Comparisons);
                while (idx < a.Length && a[idx] == v)
                {
                    r.Indexes.Add(idx);
                    idx++;
                }
            }
            return r;
        }

        // ---------- Индексно-последовательный поиск ----------

        /// <summary>
        /// Сначала последовательно просматривается индексная таблица (определяется блок),
        /// затем последовательно просматривается найденный блок основного массива.
        /// </summary>
        private static int IndexBound(int[] a, List<int> table, int value, bool strict, ref int cmp)
        {
            int j = 0;
            while (j < table.Count)
            {
                cmp++;
                int key = a[table[j]];
                bool found = strict ? key > value : key >= value;
                if (found)
                {
                    break;
                }
                j++;
            }

            int i = (j == 0) ? 0 : table[j - 1];
            while (i < a.Length)
            {
                cmp++;
                bool found = strict ? a[i] > value : a[i] >= value;
                if (found)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        public static SearchResult IndexSequentialMin(int[] a)
        {
            var r = new SearchResult();
            List<int> table = BuildIndexTable(a);
            int step = GetStep(a.Length);

            int best = 0;
            for (int j = 1; j < table.Count; j++)
            {
                r.Comparisons++;
                if (a[table[j]] < a[table[best]])
                {
                    best = j;
                }
            }

            int pos = table[best];
            int end = Math.Min(pos + step, a.Length);
            int min = pos;
            for (int i = pos + 1; i < end; i++)
            {
                r.Comparisons++;
                if (a[i] < a[min])
                {
                    min = i;
                }
            }
            r.Indexes.Add(min);
            return r;
        }

        public static SearchResult IndexSequentialGreater(int[] a, int limit)
        {
            var r = new SearchResult();
            List<int> table = BuildIndexTable(a);
            int start = IndexBound(a, table, limit, true, ref r.Comparisons);
            for (int i = start; i < a.Length; i++)
            {
                r.Indexes.Add(i);
            }
            return r;
        }

        public static SearchResult IndexSequentialMultiples(int[] a, int k)
        {
            var r = new SearchResult();
            List<int> table = BuildIndexTable(a);
            int min = a[0];
            int max = a[a.Length - 1];
            int first = (min / k) * k;
            if (first < min)
            {
                first += k;
            }
            for (int v = first; v <= max; v += k)
            {
                int idx = IndexBound(a, table, v, false, ref r.Comparisons);
                while (idx < a.Length && a[idx] == v)
                {
                    r.Indexes.Add(idx);
                    idx++;
                }
            }
            return r;
        }
    }
}
