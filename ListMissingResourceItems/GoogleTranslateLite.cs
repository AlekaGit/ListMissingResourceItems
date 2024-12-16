﻿using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    public class GoogleTranslateLite
    {
        public async Task<string> Translate(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken)
        {
            List<string?> parameters = 
            [
                    "client", "dict-chrome-ex",
                    "sl", GoogleLangCode(from),
                    "tl", GoogleLangCode(to),
                    "q", value
            ];

            return await GetHttpResponse(
                "https://clients5.google.com/translate_a/t",
                parameters,
                cancellationToken);
        }

        private static string GoogleLangCode(CultureInfo cultureInfo)
        {
            var iso1 = cultureInfo.TwoLetterISOLanguageName;
            var name = cultureInfo.Name;

            if (string.Equals(iso1, "zh", StringComparison.OrdinalIgnoreCase))
                return new[] { "zh-hant", "zh-cht", "zh-hk", "zh-mo", "zh-tw" }.Contains(name, StringComparer.OrdinalIgnoreCase) ? "zh-TW" : "zh-CN";

            if (string.Equals(name, "haw-us", StringComparison.OrdinalIgnoreCase))
                return "haw";

            return iso1;
        }

        private static async Task<string> GetHttpResponse(string baseUrl, ICollection<string?> parameters, CancellationToken cancellationToken)
        {
            var url = BuildUrl(baseUrl, parameters);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            result = result.Substring(2, result.Length - 4);
            return Regex.Unescape(result);
        }

        private static string BuildUrl(string url, ICollection<string?> pairs)
        {
            if (pairs.Count % 2 != 0)
                throw new ArgumentException("There must be an even number of strings supplied for parameters.");

            var sb = new StringBuilder(url);
            if (pairs.Count > 0)
            {
                sb.Append('?');
                sb.Append(string.Join("&", pairs.Where((s, i) => i % 2 == 0).Zip(pairs.Where((s, i) => i % 2 == 1), Format)));
            }
            return sb.ToString();

            static string Format(string? a, string? b)
            {
                return string.Concat(WebUtility.UrlEncode(a), "=", WebUtility.UrlEncode(b));
            }
        }

    }
}