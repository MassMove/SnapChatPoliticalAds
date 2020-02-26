using SCBot.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SCBot.Parsers
{
    public sealed class CampaignFileWriter : IFileWriter<Campaign>
    {
        public void Write(string filePath, IList<Campaign> items)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            try
            {
                string header = "OrganizationName,Spend,Impressions,Currency Codes,CandidateBallotInformation,PayingAdvertiserNames,Genders,AgeBrackets,CountryCodes,BillingAddresses,CreativeUrls,Interests,Regions (Included),Regions (Excluded)";
                string lines = header;

                foreach (Campaign campaign in items)
                {
                    string line = FormatItem(campaign.organizationName) + ",";
                    line += campaign.spend + ",";
                    line += campaign.impressions + ",";
                    line += FormatItem(campaign.currencyCodes[0]) + ",";
                    line += FormatItem(campaign.candidateBallotNames[0]) + ",";
                    line += FormatItem(campaign.payingAdvertiserNames[0]) + ",";
                    line += FormatItem(campaign.genders[0]) + ",";
                    line += FormatItem(campaign.ageBrackets[0]) + ",";
                    line += FormatItem(campaign.countryCodes[0]) + ",";
                    line += FormatItem(campaign.billingAddresses[0]) + ",";
                    line += FormatItem(campaign.creativeUrls[0]) + ",";
                    line += FormatItem(campaign.interests[0]) + ",";
                    line += FormatItem(campaign.includedRegions[0]) + ",";
                    line += FormatItem(campaign.excludedRegions[0]);

                    lines += "\r\n" + line;
                }

                File.WriteAllText(filePath, lines);
            }
            catch (IOException)
            {
                throw;
            }
        }

        private static string FormatItem(string item)
        {
            char[] chars = { '\t', '\r', '\n', '\"', ',' };

            if (item.IndexOfAny(chars) >= 0)
            {
                item = '\"' + item.Replace("\"", "\"\"") + '\"';
            }
            return item;
        }

        private static string FormatList(List<string> listItems)
        {
            if (listItems.Count == 0)
            {
                return string.Empty;
            }

            if (listItems.Count == 1)
            {
                return FormatItem(listItems[0]);
            }

            string list = string.Empty;

            foreach (string listItem in listItems)
            {
                if (!string.IsNullOrEmpty(listItem))
                {
                    list += listItem + ";";
                }
            }
            return FormatItem(list);
        }
    }
}