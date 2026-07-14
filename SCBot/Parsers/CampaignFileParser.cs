using Microsoft.VisualBasic.FileIO;
using SCBot.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;

namespace SCBot.Parsers
{
    public sealed class CampaignFileParser : IFileParser<Campaign>
    {
        public void Download(string dataFile, string url)
        {
            using (WebClient webClient = new WebClient())
            {
                var scData = webClient.DownloadData(url);
                var zipStream = new MemoryStream(scData);

                using (ZipArchive archive = new ZipArchive(zipStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name == "PoliticalAds.csv")
                        {
                            entry.ExtractToFile(dataFile, true);
                            break;
                        }
                    }
                }
            }

            NormalizeRawCsv(dataFile);
        }

        private static void NormalizeRawCsv(string dataFile)
        {
            if (!File.Exists(dataFile))
            {
                return;
            }

            var lines = File.ReadAllText(dataFile).Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (lines.Length <= 1)
            {
                return;
            }

            var header = lines[0];
            var records = new List<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (i == lines.Length - 1 && line.Length == 0)
                {
                    continue; // trailing newline produces a final empty element
                }

                if (IsRecordStart(line) || records.Count == 0)
                {
                    records.Add(line);
                }
                else
                {
                    records[records.Count - 1] += "\r\n" + line;
                }
            }

            records.Sort(StringComparer.Ordinal);

            File.WriteAllText(dataFile, header + "\r\n" + string.Join("\r\n", records) + "\r\n");
        }

        // A record starts with a 64-character lowercase hex ADID followed by a comma.
        private static bool IsRecordStart(string line)
        {
            if (line.Length < 65 || line[64] != ',')
            {
                return false;
            }
            for (int i = 0; i < 64; i++)
            {
                char c = line[i];
                bool isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!isHex)
                {
                    return false;
                }
            }
            return true;
        }

        public List<Campaign> Parse(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            List<Campaign> campaigns = new List<Campaign>();
            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.ReadFields(); // skip header

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        Campaign campaign = ParseCampaign(fields);

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

                return campaigns
                    .OrderByDescending(c => c.impressions)
                    .ThenBy(c => c.payingAdvertiserName, StringComparer.Ordinal)
                    .ToList();
            }
            catch (IOException)
            {
                // Don't swallow into a null return: the caller immediately
                // enumerates the result, and an empty/partial run must not
                // overwrite good data. Fail loudly so the nightly job stops
                // before the commit step.
                throw;
            }
        }

        public static Campaign ParseCampaign(string[] fields)
        {
            var campaign = new Campaign();
            campaign.organizationName = fields[7].Replace(",", " ");

            long spend;
            long.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out spend);
            campaign.spend = spend;

            long impressions;
            long.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out impressions);
            campaign.impressions = impressions;

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
    }
}