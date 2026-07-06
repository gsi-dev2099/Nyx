using System.Text;

namespace CRM.ApiHub.Domain.Utils;

public static class StringSanitizer
{
    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            // Strip out control characters and invalid chars (like U+0081)
            if (c == '\u0081' || char.IsControl(c))
            {
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
}
