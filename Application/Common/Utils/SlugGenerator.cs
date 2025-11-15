using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Common.Utils
{
    public static class SlugGenerator
    {
        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var s = sb.ToString().Normalize(NormalizationForm.FormC);
            s = Regex.Replace(s, @"[^a-z0-9\s\-]", ""); // remove non-alnum
            s = Regex.Replace(s, @"\s+", "-");          // spaces -> dash
            s = Regex.Replace(s, @"-+", "-").Trim('-'); // collapse
            return s;
        }
    }
}
