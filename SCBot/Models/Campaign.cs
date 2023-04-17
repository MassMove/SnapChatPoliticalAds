using System.Collections.Generic;

namespace SCBot.Models
{
    public sealed class Campaign
    {
        public List<string> ageBrackets { get; set; } = new List<string>();
        public List<string> billingAddresses { get; set; } = new List<string>();
        public string candidateBallotName;
        public List<string> candidateBallotNames { get; set; } = new List<string>();
        public List<string> countryCodes { get; set; } = new List<string>();
        public List<string> creativeUrls { get; set; } = new List<string>();
        public string creativeUrlsSort;
        public List<string> currencyCodes { get; set; } = new List<string>();
        public List<string> excludedRegions { get; set; } = new List<string>();
        public List<string> genders { get; set; } = new List<string>();
        public long impressions { get; set; } = 0;
        public List<string> includedRegions { get; set; } = new List<string>();
        public List<string> interests { get; set; } = new List<string>();
        public string organizationName { get; set; }
        public string payingAdvertiserName;
        public List<string> payingAdvertiserNames { get; set; } = new List<string>();
        public long spend { get; set; } = 0;
    }
}