using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using SCBot.Models;
using SCBot.Parsers;

namespace SCBot
{
    internal class Program
    {
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

        private static void Main(string[] args)
        {
            string readMe = "# SCBot\r\n\r\n";
            readMe += "A bot to suMMarize the [Snap Chat Political Ads Library](https://www.snap.com/en-US/political-ads).\r\n\r\n";
            readMe += "Source and summarized data in CSV format: [/SCData](https://github.com/MassMove/SCBot/tree/master/SCData).\r\n\r\n";
            readMe += "Last run: " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".\r\n\r\n";
            try
            {
                for (int year = 2020; year >= 2018; year--)
                {
                    Console.WriteLine($"Campaign Data for:{year}");

                    var dataFile = $"../../../../SCData/{year}.csv";
                    var dataSummaryFile = $"../../../../SCData/{year}_suMMarized.csv";

                    var campaignFileParser = new CampaignFileParser();

                    var campaigns = campaignFileParser.Parse(dataFile);

                    if (campaigns.Count == 0)
                    {
                        Console.WriteLine("No Campaign Data Found.\r\n");
                        return;
                    }

                    campaigns.OrderByDescending(c => c.spend);

                    var campaignSummaryWriter = new CampaignFileWriter();

                    campaignSummaryWriter.Write(dataSummaryFile, campaigns);
                }
            }
            catch (Exception)
            {
                throw;
            }

            /*
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

                        Campaign campaign = new Campaign
                        {
                            organizationName = fields[7].Replace(",", " ")
                        };
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

                string header = "OrganizationName,Spend,Impressions,Currency Codes,CandidateBallotInformation,PayingAdvertiserNames,Genders,AgeBrackets,CountryCodes,BillingAddresses,CreativeUrls,Interests,Regions (Included),Regions (Excluded)";
                string lines = header;

                foreach (Campaign campaign in campaigns)
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
                File.WriteAllText("../../../../SCData/" + year + "_suMMarized.csv", lines);

                readMe += "## " + year + " \r\n";
                readMe += "|OrganizationName|Spent|CandidateBallotInformation|PayingAdvertiserNames|CreativeUrls|Genders|AgeBrackets|CountryCodes|BillingAddresses|Impressions|Currency Codes|\r\n";
                readMe += "|:---|---:|---:|:---|:---|:---|:---|:---|:---|:---|:---|\r\n";

                List<Campaign> top25 = campaigns.GetRange(0, 25);
                foreach (Campaign campaign in top25)
                {
                    string line = "|" + FormatItem(campaign.organizationName) + "|";
                    line += campaign.spend + "|";
                    line += FormatList(campaign.candidateBallotNames) + "|";
                    line += FormatList(campaign.payingAdvertiserNames) + "|";

                    for (int i = 0; i < campaign.creativeUrls.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(campaign.creativeUrls[i]))
                        {
                            line += "[" + i + "](" + campaign.creativeUrls[i] + "),";
                        }
                    }

                    line = line.TrimEnd(',') + "|";

                    for (int i = 0; i < campaign.genders.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(campaign.genders[i]))
                        {
                            line += campaign.genders[i] + ", ";
                        }
                    }

                    line = line.TrimEnd(' ').TrimEnd(',') + "|";

                    for (int i = 0; i < campaign.ageBrackets.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(campaign.ageBrackets[i]))
                        {
                            line += campaign.ageBrackets[i] + ", ";
                        }
                    }

                    line = line.TrimEnd(' ').TrimEnd(',') + "|";

                    line += FormatList(campaign.countryCodes) + "|";
                    line += FormatList(campaign.billingAddresses) + "|";

                    line += campaign.impressions + "|";
                    line += FormatList(campaign.currencyCodes) + "|";

                    readMe += line + "\r\n";
                }
                readMe += "\r\n";
                File.WriteAllText("../../../../README.md", readMe);
            }
            */
        }

        private static void UpdateReadME(IList<Campaign> campaignList)
        {
            string readMe = "# SCBot\r\n\r\n";
            readMe += "A bot to suMMarize the [Snap Chat Political Ads Library](https://www.snap.com/en-US/political-ads).\r\n\r\n";
            readMe += "Source and summarized data in CSV format: [/SCData](https://github.com/MassMove/SCBot/tree/master/SCData).\r\n\r\n";
            readMe += "Last run: " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".\r\n\r\n";
        }
    }
}