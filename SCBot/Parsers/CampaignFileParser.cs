using Microsoft.VisualBasic.FileIO;
using SCBot.Models;
using System;
using System.Collections.Generic;
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
                            existingCampaign.ageBrackets.Add(campaign.ageBrackets[0
                                ]);
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

                return campaigns.OrderByDescending(c => c.spend).ToList();
            }
            catch (IOException)
            {
                return default;
            }
        }

        public static Campaign ParseCampaign(string[] fields)
        {
            var campaign = new Campaign();
            campaign.organizationName = fields[7].Replace(",", " ");

            long spend;
            long.TryParse(fields[3], out spend);    
            campaign.spend = spend;

            long impressions;
            long.TryParse(fields[4], out impressions);
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