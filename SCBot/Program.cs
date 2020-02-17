using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCBot
{
    class Program
    {
        public class Campaign
        {
            public String organizationName;
            public long spend = 0;
            public long impressions = 0;
            public List<String> creativeUrls = new List<String>();
            public List<String> currencyCodes = new List<String>();
            public List<String> billingAddresses = new List<String>();
            public List<String> candidateBallotNames = new List<String>();
            public List<String> payingAdvertiserNames = new List<String>();
            public List<String> genders = new List<String>();
            public List<String> ageBrackets = new List<String>();
            public List<String> countryCodes = new List<String>();
            public List<String> includedRegions = new List<String>();
            public List<String> excludedRegions = new List<String>();
            public List<String> interests = new List<String>();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("This is our world now");
            String readMe = "# SCBot\r\n\r\n";
            readMe += "A bot to suMMarize the [Snap Chat Political Ads Library](https://www.snap.com/en-US/political-ads).\r\n";
            readMe += "Source and summarized data in CSV format: [/SCData](https://github.com/MassMove/SCBot/tree/master/SCData).\r\n\r\n";

            for (int year = 2020; year >= 2018; year--)
            {
                List<Campaign> campaigns = new List<Campaign>();
                using (TextFieldParser parser = new TextFieldParser("../../../../SCData/" + year + ".csv"))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.ReadFields(); // skip header

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        Campaign campaign = new Campaign();
                        campaign.organizationName = fields[7].Replace(",", " ");
                        long.TryParse(fields[3], out campaign.spend);
                        long.TryParse(fields[4], out campaign.impressions);
                        campaign.creativeUrls.Add(fields[1]);
                        campaign.currencyCodes.Add(fields[2]);
                        campaign.billingAddresses.Add(fields[8]);
                        campaign.candidateBallotNames.Add(fields[9]);
                        campaign.payingAdvertiserNames.Add(fields[10]);
                        campaign.genders.Add(fields[11]);
                        campaign.ageBrackets.Add(fields[12]);
                        campaign.countryCodes.Add(fields[13]);
                        campaign.includedRegions.Add(fields[14]);
                        campaign.excludedRegions.Add(fields[15]);
                        campaign.interests.Add(fields[26]);

                        Campaign existingCampaign = campaigns.Find(x => x.organizationName == campaign.organizationName);
                        if (existingCampaign == null)
                        {
                            campaigns.Add(campaign);
                            continue;
                        }

                        existingCampaign.spend += campaign.spend;
                        existingCampaign.impressions += campaign.impressions;

                        if (existingCampaign.creativeUrls.Find(x => x == campaign.creativeUrls[0]) == null)
                        {
                            existingCampaign.creativeUrls.Add(campaign.creativeUrls[0]);
                        }
                        if (existingCampaign.currencyCodes.Find(x => x == campaign.currencyCodes[0]) == null)
                        {
                            existingCampaign.currencyCodes.Add(campaign.currencyCodes[0]);
                        }
                        if (existingCampaign.billingAddresses.Find(x => x == campaign.billingAddresses[0]) == null)
                        {
                            existingCampaign.billingAddresses.Add(campaign.billingAddresses[0]);
                        }
                        if (existingCampaign.candidateBallotNames.Find(x => x == campaign.candidateBallotNames[0]) == null)
                        {
                            existingCampaign.candidateBallotNames.Add(campaign.candidateBallotNames[0]);
                        }
                        if (existingCampaign.payingAdvertiserNames.Find(x => x == campaign.payingAdvertiserNames[0]) == null)
                        {
                            existingCampaign.payingAdvertiserNames.Add(campaign.payingAdvertiserNames[0]);
                        }
                        if (existingCampaign.genders.Find(x => x == campaign.genders[0]) == null)
                        {
                            existingCampaign.genders.Add(campaign.genders[0]);
                        }
                        if (existingCampaign.ageBrackets.Find(x => x == campaign.ageBrackets[0]) == null)
                        {
                            existingCampaign.ageBrackets.Add(campaign.ageBrackets[0]);
                        }
                        if (existingCampaign.countryCodes.Find(x => x == campaign.countryCodes[0]) == null)
                        {
                            existingCampaign.countryCodes.Add(campaign.countryCodes[0]);
                        }
                        if (existingCampaign.includedRegions.Find(x => x == campaign.includedRegions[0]) == null)
                        {
                            existingCampaign.includedRegions.Add(campaign.includedRegions[0]);
                        }
                        if (existingCampaign.excludedRegions.Find(x => x == campaign.excludedRegions[0]) == null)
                        {
                            existingCampaign.excludedRegions.Add(campaign.excludedRegions[0]);
                        }
                        if (existingCampaign.interests.Find(x => x == campaign.interests[0]) == null)
                        {
                            existingCampaign.interests.Add(campaign.interests[0]);
                        }
                    }
                }

                campaigns = campaigns.OrderByDescending(c => c.spend).ToList();

                String header = "OrganizationName,Spend,Impressions,Currency Codes,CandidateBallotInformation,PayingAdvertiserNames,Genders,AgeBrackets,CountryCodes,BillingAddresses,CreativeUrls,Interests,Regions (Included),Regions (Excluded)";
                String lines = header;
                foreach (Campaign campaign in campaigns)
                {
                    String line = formatItem(campaign.organizationName) + ",";
                    line += campaign.spend + ",";
                    line += campaign.impressions + ",";
                    line += formatItem(campaign.currencyCodes[0]) + ",";
                    line += formatItem(campaign.candidateBallotNames[0]) + ",";
                    line += formatItem(campaign.payingAdvertiserNames[0]) + ",";
                    line += formatItem(campaign.genders[0]) + ",";
                    line += formatItem(campaign.ageBrackets[0]) + ",";
                    line += formatItem(campaign.countryCodes[0]) + ",";
                    line += formatItem(campaign.billingAddresses[0]) + ",";
                    line += formatItem(campaign.creativeUrls[0]) + ",";
                    line += formatItem(campaign.interests[0]) + ",";
                    line += formatItem(campaign.includedRegions[0]) + ",";
                    line += formatItem(campaign.excludedRegions[0]);

                    lines += "\r\n" + line;
                }
                File.WriteAllText("../../../../SCData/" + year + "_suMMarized.csv", lines);

                readMe += "## " + year + " \r\n";
                readMe += "|OrganizationName|Spent|CandidateBallotInformation|PayingAdvertiserNames|CreativeUrls|Genders|AgeBrackets|CountryCodes|BillingAddresses|Impressions|Currency Codes|\r\n";
                readMe += "|:---|---:|---:|:---|:---|:---|:---|:---|:---|:---|:---|\r\n";
                foreach (Campaign campaign in campaigns)
                {
                    if (campaign.spend < 10000)
                    {
                        break;
                    }
                    String line = "|" + formatItem(campaign.organizationName) + "|";
                    line += campaign.spend + "|";
                    line += formatList(campaign.candidateBallotNames) + "|";
                    line += formatList(campaign.payingAdvertiserNames) + "|";

                    for (int i = 0; i < campaign.creativeUrls.Count; i++)
                    {
                        line += "[" + i + "](" + campaign.creativeUrls[i] + "),";
                    }
                    line = line.TrimEnd(',') + "|";

                    for (int i = 0; i < campaign.genders.Count; i++)
                    {
                        line += campaign.genders[i] + ", ";
                    }
                    line = line.TrimEnd(',') + "|";

                    for (int i = 0; i < campaign.ageBrackets.Count; i++)
                    {
                        line += campaign.ageBrackets[i] + ", ";
                    }
                    line = line.TrimEnd(',') + "|";

                    line += formatList(campaign.countryCodes) + "|";
                    line += formatList(campaign.billingAddresses) + "|";

                    line += campaign.impressions + "|";
                    line += formatList(campaign.currencyCodes) + "|";

                    readMe += line + "\r\n";
                }
                readMe += "\r\n";
                File.WriteAllText("../../../../README.md", readMe);
            }
        }

        private static String formatList(List<String> listItems)
        {
            if (listItems.Count == 0)
            {
                return "";
            }

            if (listItems.Count == 1)
            {
                return formatItem(listItems[0]);
            }

            String list = "";
            foreach (String listItem in listItems)
            {
                if (listItem != "")
                {
                    list += listItem + ";";
                }
            }
            return formatItem(list);
        }

        private static String formatItem(String item)
        {
            char[] chars = { '\t', '\r', '\n', '\"', ',' };
            if (item.IndexOfAny(chars) >= 0)
            {
                item = '\"' + item.Replace("\"", "\"\"") + '\"';
            }
            return item;
        }

    }
}
