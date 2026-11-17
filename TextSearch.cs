using System;
using System.Collections.Generic;
using System.Text;

namespace Lab7Search
{
    /// <summary>Задача 1. Поиск вхождения строки в текст.</summary>
    public static class TextSearch
    {
        /// <summary>Разбивает текст на слова (буквы, цифры, дефис и подчёркивание).</summary>
        public static List<string> SplitWords(string text)
        {
            var words = new List<string>();
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
                else if (sb.Length > 0)
                {
                    words.Add(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
            {
                words.Add(sb.ToString());
            }
            return words;
        }

        /// <summary>Прямой (наивный) поиск подстроки в слове.</summary>
        public static bool ContainsSubstring(string word, string pattern, bool ignoreCase)
        {
            if (ignoreCase)
            {
                word = word.ToLowerInvariant();
                pattern = pattern.ToLowerInvariant();
            }
            if (pattern.Length > word.Length)
            {
                return false;
            }
            for (int i = 0; i <= word.Length - pattern.Length; i++)
            {
                int j = 0;
                while (j < pattern.Length && word[i + j] == pattern[j])
                {
                    j++;
                }
                if (j == pattern.Length)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Возвращает все (неповторяющиеся) слова текста, содержащие искомую строку.</summary>
        public static List<string> FindWords(string text, string pattern, bool ignoreCase)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (string word in SplitWords(text))
            {
                if (ContainsSubstring(word, pattern, ignoreCase) && seen.Add(word))
                {
                    result.Add(word);
                }
            }
            return result;
        }
    }
}
