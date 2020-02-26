using SCBot.Models;
using SCBot.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

                    readMe += "## " + year + " \r\n";
                    readMe += "|OrganizationName|Spent|CandidateBallotInformation|PayingAdvertiserNames|CreativeUrls|Genders|AgeBrackets|CountryCodes|BillingAddresses|Impressions|Currency Codes|\r\n";
                    readMe += "|:---|---:|---:|:---|:---|:---|:---|:---|:---|:---|:---|\r\n";

                    List<Campaign> top25 = campaigns.Take(25).ToList();

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
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}