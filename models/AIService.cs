using System.Text.RegularExpressions;

namespace SecureCommECC.Services
{
    public class AIService
    {
        public string AnalyzeMessage(string message)
        {
            var categories = new System.Collections.Generic.List<string>();

            // Basic AI simulation for categorizing message content
            // This can be expanded significantly with more complex regex, NLP libraries, or actual AI model integration

            // Financial detection
            if (Regex.IsMatch(message, @"\b(bank|account|credit|card|debit|loan|finance|investment|stock|crypto|bitcoin|ethereum|wallet|transaction|balance|payment|routing|swift)\b", RegexOptions.IgnoreCase))
                categories.Add("Financial");

            // Confidential/Security detection
            if (Regex.IsMatch(message, @"\b(password|secret|key|login|credential|auth|token|api_key|pin|code|access|secure|private|confidential|NDA|top secret)\b", RegexOptions.IgnoreCase))
                categories.Add("Confidential");

            // Personal Information (PII) detection
            if (Regex.IsMatch(message, @"\b(email|phone|address|ssn|social\s+security|passport|driver\s+license|birthdate|name|personal|home\s+address|date\s+of\s+birth)\b", RegexOptions.IgnoreCase))
                categories.Add("Personal Information (PII)");

            // Health/Medical detection
            if (Regex.IsMatch(message, @"\b(health|medical|doctor|hospital|diagnosis|treatment|medicine|prescription|symptom|illness|patient|clinic|hipaa)\b", RegexOptions.IgnoreCase))
                categories.Add("Health (PHI)");

            // Threat/Malicious intent detection (very basic example)
            if (Regex.IsMatch(message, @"\b(attack|exploit|vulnerability|malware|virus|trojan|phishing|ransom|hack|breach)\b", RegexOptions.IgnoreCase))
                categories.Add("Potential Threat/Security Alert");

            // Urgent/Critical communication
            if (Regex.IsMatch(message, @"\b(urgent|critical|immediate|warning|alert|ASAP)\b", RegexOptions.IgnoreCase))
                categories.Add("Urgent Communication");

            // Length-based (heuristic for potentially sensitive long documents)
            if (message.Length > 1000)
                categories.Add("Long Message - May require careful review");
            else if (message.Length > 500)
                categories.Add("Medium Length - Review for Detail");


            // Default category if no specific patterns match
            if (categories.Count == 0)
                categories.Add("General/Informational");

            // Return a joined string of all detected categories
            return string.Join(" | ", categories);
        }
    }
}