using Microsoft.VisualBasic.FileIO;
using SCBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCBot.Parsers
{
    public sealed class CampaignFileWriter : IFileWriter<Campaign>
    {
        int urlStartIndex;

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

        public string WriteReadMe(IList<Campaign> campaigns, string readMe, int year)
        {
            readMe += $"## [{year}]({year}/README.md) \r\n";
            readMe += "|Advertiser|Spent|Impressions|Genders|Age Brackets|Country Codes|\r\n";
            readMe += "|:---|---:|---:|:---|:---|:---|\r\n";

            var top25 = campaigns;

            if (campaigns.Count > 25)
            {
                top25 = top25.ToList().GetRange(0, 25);
            }
            foreach (Campaign campaign in top25)
            {
                readMe += FormatLine(campaign, year, false, false).Replace("\"", "") + "\r\n";
                Console.WriteLine(campaign.payingAdvertiserName + ": " + campaign.spend);
            }
            readMe += "\r\n";
            File.WriteAllText("../../../../README.md", readMe);

            return readMe;
        }

        public void WriteReadMeYear(IList<Campaign> campaigns, int year, string filePath)
        {
            var readMeYear = "## " + year + " \r\n";
            readMeYear += "|Advertiser|Spent|Impressions|Genders|Age Brackets|Country Codes|\r\n";
            readMeYear += "|:---|---:|---:|:---|:---|:---|\r\n";

            if (!Directory.Exists("../../../../" + year))
            {
                Directory.CreateDirectory("../../../../" + year);
            }

            Console.WriteLine("\r\n" + year + " details");

            var advertiserCampaigns = new List<Campaign>();
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields();

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    Campaign campaign = CampaignFileParser.ParseCampaign(fields);
                    campaign.creativeUrlsSort = string.Join(",", campaign.creativeUrls);
                    advertiserCampaigns.Add(campaign);
                }
            }

            foreach (var campaign in campaigns)
            {
                readMeYear += FormatLine(campaign, 1, false, false).Replace("\"", "") + "\r\n";

                var readMeAdvertiser = GenerateAdvertiserTable(campaign.payingAdvertiserName, year, advertiserCampaigns);

                var filename = string.Join("_", campaign.payingAdvertiserName.Split(Path.GetInvalidFileNameChars()));
                filename = string.Join("_", filename.Split(" "));
                File.WriteAllText("../../../../" + year + "/" + filename + ".md", readMeAdvertiser);
                Console.WriteLine(campaign.payingAdvertiserName + ": " + campaign.spend);
            }
            readMeYear += "\r\n";
            File.WriteAllText("../../../../" + year + "/README.md", readMeYear);
        }

        private string GenerateAdvertiserTable(string advertiser, int year, IList<Campaign> advertiserCampaigns)
        {
            urlStartIndex = 0;

            var campaigns = advertiserCampaigns.Where(c => c.payingAdvertiserName == advertiser).ToList();

            var readMeAdvertiser = "## " + year + " - " + advertiser + " \r\n";
            readMeAdvertiser += $"**Spent**: {campaigns.Select(c => c.spend).Sum().ToString("N")}\r\n";
            readMeAdvertiser += "\r\n";
            readMeAdvertiser += $"**Impressions**: {campaigns.Select(c => c.impressions).Sum().ToString("N0")}\r\n";
            readMeAdvertiser += "\r\n";
            readMeAdvertiser += $"**Billing Addresses**: {FormatAddresses(campaigns)}\r\n";
            readMeAdvertiser += "\r\n";

            var includeAddresses = GetAddressCount(campaigns) > 1 ? true : false;

            readMeAdvertiser += "|Organization|Spent|Urls|Impressions|Genders|Age Brackets|Country Codes|";
            if (includeAddresses)
            {
                readMeAdvertiser += "Billing Addresses|";
            }
            readMeAdvertiser += "\r\n";
            readMeAdvertiser += "|:---|---:|:---|---:|:---|:---|:---|";
            if (includeAddresses)
            {
                readMeAdvertiser += ":---|";
            }
            readMeAdvertiser += "\r\n";

            foreach (var campaign in campaigns.
                OrderByDescending(c => c.impressions).
                ThenByDescending(c => c.spend).
                ThenBy(c => c.creativeUrlsSort))
            {
                readMeAdvertiser += FormatLine(campaign, 0, true, includeAddresses).Replace("\"", "") + "\r\n";
            }

            return readMeAdvertiser;
        }

        private static string FormatAddresses(IList<Campaign> campaigns)
        {
            var billingAddresses = new List<string>();

            foreach (var campaign in campaigns)
            {
                foreach (var billingAddress in campaign.billingAddresses)
                {
                    if (!billingAddresses.Contains(billingAddress))
                    {
                        billingAddresses.Add(billingAddress);
                    }
                }
            }

            if (billingAddresses.Count > 1)
            {                
                return "\r\n- " + string.Join("\r\n- ", billingAddresses.Distinct().OrderBy(a => a));
            }

            return string.Join(", ", billingAddresses.Distinct());
        }

        private static int GetAddressCount(IList<Campaign> campaigns)
        {
            var billingAddresses = new List<string>();

            foreach (var campaign in campaigns)
            {
                foreach (var billingAddress in campaign.billingAddresses)
                {
                    if (!billingAddresses.Contains(billingAddress))
                    {
                        billingAddresses.Add(billingAddress);
                    }
                }
            }

            return billingAddresses.Count;
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

        private string FormatLine(Campaign campaign, int year, bool groupUrls, bool includeAddresses)
        {
            var filename = string.Join("_", campaign.payingAdvertiserName.Split(Path.GetInvalidFileNameChars()));
            filename = string.Join("_", filename.Split(" "));

            var line = "|";

            if (year == 0)
            {
                line += FormatItem(campaign.organizationName);
            }
            else if (year == 1)
            {
                line += "[" + campaign.payingAdvertiserName + "](" + filename + ".md)";
                line += " - " + FormatItem(campaign.organizationName);
            }
            else
            {
                line += "[" + campaign.payingAdvertiserName + "](" + year + "/" + filename + ".md)";
                line += " - " + FormatItem(campaign.organizationName);
            }

            var ballotNames = FormatList(campaign.candidateBallotNames);
            if (ballotNames != "")
            {
                line += ": " + FormatList(campaign.candidateBallotNames);
            }

            line += "|";

            line += campaign.spend.ToString("N") + " " + FormatList(campaign.currencyCodes) + "|";

            if (year == 0)
            {
                if (groupUrls)
                {
                    var formattedUrls = FormatUrls(campaign.creativeUrls, urlStartIndex) + "|";
                    urlStartIndex += formattedUrls.Split(",").Length;
                    line += formattedUrls;
                }
                else
                {
                    line += FormatUrls(campaign.creativeUrls, 0) + "|";
                }
            }
            line += campaign.impressions.ToString("N0") + "|";
            line += FormatList(campaign.genders) + "|";
            line += FormatList(campaign.ageBrackets) + "|";
            line += FormatList(campaign.countryCodes) + "|";
            if (includeAddresses)
            {
                line += FormatList(campaign.billingAddresses) + "|";
            }
            return line;
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

            var list = string.Empty;
            foreach (var listItem in listItems.OrderBy(l => l))
            {
                if (listItem != "")
                {
                    list += listItem + "; ";
                }
            }
            list = list.TrimEnd(' ').TrimEnd(';');
            list = FormatItem(list).Replace(";", ",");
            return FormatItem(list);
        }

        private static string FormatUrls(List<string> creativeUrls, int startIndex)
        {
            var urls = "";
            var urlSpacing = 0;
            var urlIndex = startIndex;

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