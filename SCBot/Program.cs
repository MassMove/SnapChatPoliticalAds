using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace SCBot
{
    class Program
    {
        public class Campaign
        {
            public String organizationName;
            public String candidateBallotName;
            public String payingAdvertiserName;
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
            readMe += "A bot to suMMarize the [Snap Chat Political Ads Library](https://www.snap.com/en-US/political-ads).\r\n\r\n";
            readMe += "Source and summarized data in CSV format: [/SCData](https://github.com/MassMove/SCBot/tree/master/SCData).\r\n\r\n";
            readMe += "Last run: " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".\r\n\r\n";

            for (int year = DateTime.Now.Year; year >= 2018; year--)
            {
                Console.WriteLine("\r\n" + year);

                var filePath = "../../../../SCData/" + year + ".csv";
                using (WebClient webClient = new WebClient())
                {
                    var scData = webClient.DownloadData("https://storage.googleapis.com/ad-manager-political-ads-dump/political/" + year + "/PoliticalAds.zip");
                    var zipStream = new MemoryStream(scData);

                    using (ZipArchive archive = new ZipArchive(zipStream))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.Name == "PoliticalAds.csv")
                            {
                                entry.ExtractToFile(filePath, true);
                                break;
                            }
                        }
                    }
                }

                List<Campaign> campaigns = new List<Campaign>();
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.ReadFields(); // skip header

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        Campaign campaign = parseCampaign(fields);

                        Campaign existingCampaign = campaigns.Find(x => x.payingAdvertiserName == campaign.payingAdvertiserName);

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

                readMe += "## [" + year + "](" + year + ") \r\n";
                readMe += "|OrganizationName|Spent|PayingAdvertiserNames|CreativeUrls|Impressions|Genders|AgeBrackets|CountryCodes|BillingAddresses|CandidateBallotInformation|\r\n";
                readMe += "|:---|---:|:---|:---|---:|:---|:---|:---|:---|:---|\r\n";

                List<Campaign> top25 = campaigns;
                
                if (campaigns.Count > 25)
                {
                    top25 = campaigns.GetRange(0, 25);
                }
                foreach (Campaign campaign in top25)
                {
                    readMe += formatLine(campaign, year) + "\r\n";
                    Console.WriteLine(campaign.payingAdvertiserName + ": " + campaign.spend);
                }
                readMe += "\r\n";
                File.WriteAllText("../../../../README.md", readMe);

                var readMeYear = "## " + year + " \r\n";
                readMeYear += "|OrganizationName|Spent|PayingAdvertiserNames|CreativeUrls|Impressions|Genders|AgeBrackets|CountryCodes|BillingAddresses|CandidateBallotInformation|\r\n";
                readMeYear += "|:---|---:|:---|:---|---:|:---|:---|:---|:---|:---|\r\n";

                foreach (Campaign campaign in campaigns)
                {
                    readMeYear += formatLine(campaign, year) + "\r\n";

                    var readMeAdvertiser = "## " + year + " - " + campaign.payingAdvertiserName + " \r\n";
                    readMeAdvertiser += "|OrganizationName|Spent|PayingAdvertiserNames|CreativeUrls|Impressions|Genders|AgeBrackets|CountryCodes|BillingAddresses|CandidateBallotInformation|\r\n";
                    readMeAdvertiser += "|:---|---:|:---|:---|---:|:---|:---|:---|:---|:---|\r\n";
                    readMeAdvertiser += generateAdvertiserTable(filePath, campaign.payingAdvertiserName, year);

                    var filename = string.Join("", campaign.payingAdvertiserName.Split(Path.GetInvalidFileNameChars()));
                    File.WriteAllText("../../../../" + year + "/" + filename + ".md", readMeAdvertiser);
                }
                readMeYear += "\r\n";
                if (!Directory.Exists("../../../../" + year))
                {
                    Directory.CreateDirectory("../../../../" + year);
                }
                File.WriteAllText("../../../../" + year + "/README.md", readMeYear);
            }
        }

        private static Campaign parseCampaign(string[] fields)
        {
            var campaign = new Campaign();
            campaign.organizationName = fields[7].Replace(",", " ");
            long.TryParse(fields[3], out campaign.spend);
            long.TryParse(fields[4], out campaign.impressions);
            campaign.creativeUrls.Add(fields[1]);
            campaign.currencyCodes.Add(fields[2]);
            campaign.billingAddresses.Add(fields[8]);
            campaign.candidateBallotName = fields[9];
            campaign.candidateBallotNames.Add(fields[9]);
            campaign.payingAdvertiserNames.Add(fields[10]);
            campaign.payingAdvertiserName = fields[10];
            campaign.genders.Add(fields[15]);
            campaign.ageBrackets.Add(fields[16]);
            campaign.countryCodes.Add(fields[17]);
            campaign.includedRegions.Add(fields[18]);
            campaign.excludedRegions.Add(fields[19]);
            campaign.interests.Add(fields[30]);
            return campaign;
        }

        private static string generateAdvertiserTable(string filePath, string advertiser, int year)
        {
            var advertiserTable = "";
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); // skip header

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    Campaign campaign = parseCampaign(fields);
                    if (campaign.payingAdvertiserName == advertiser)
                    {
                        advertiserTable += formatLine(campaign, year) + "\r\n";
                    }
                }
            }

            return advertiserTable;
        }

        private static String formatLine(Campaign campaign, int year)
        {
            var line = "|" + formatItem(campaign.organizationName) + "|";
            line += campaign.spend.ToString("N") + " " + formatList(campaign.currencyCodes) + "|";

            var filename = string.Join("", campaign.payingAdvertiserName.Split(Path.GetInvalidFileNameChars()));
            line += "[" + campaign.payingAdvertiserName + "](" + year + "/" + filename + ".md)|";
            line += formatUrls(campaign.creativeUrls) + "|";
            line += campaign.impressions.ToString("N0") + "|";
            line += formatList(campaign.genders) + "|";
            line += formatList(campaign.ageBrackets) + "|";
            line += formatList(campaign.countryCodes) + "|";
            line += formatList(campaign.billingAddresses) + "|";
            line += formatList(campaign.candidateBallotNames) + "|";
            return line;
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
                    list += listItem + "; ";
                }
            }
            list = list.TrimEnd(' ').TrimEnd(';');
            list = formatItem(list).Replace(";", ",");
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

        private static string formatUrls(List<string> creativeUrls)
        {
            var urls = "";
            var urlSpacing = 0;
            var urlIndex = 0;

            var mergedUrls = new List<string>();
            foreach (var url in creativeUrls)
            {
                if (url != "")
                {
                    if (url.Contains(";"))
                    {
                        var subUrls = url.Split(';');
                        foreach (var subUrl in subUrls)
                        {
                            mergedUrls.Add(subUrl);
                        }
                    }
                    else
                    {
                        mergedUrls.Add(url);
                    }
                }
            }

            foreach (var url in mergedUrls)
            {
                urls += "[" + urlIndex + "](" + url + "),";
                urlSpacing++;
                urlIndex++;
                if (urlSpacing >= 16)
                {
                    urls += " ";
                    urlSpacing = 0;
                }
            }

            urls = urls.TrimEnd(' ').TrimEnd(',');
            return urls;
        }

    }
}
