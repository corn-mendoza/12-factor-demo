using System.Collections.Generic;

namespace Pivotal.Workshop.Models
{
    public class SentimentResponse
    {
        public List<Dictionary<string, string>> Documents { get; set; }
    }
}
