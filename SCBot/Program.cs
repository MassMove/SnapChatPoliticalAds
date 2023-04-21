using SCBot.Parsers;
using System;
using System.Linq;

namespace SCBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is our world now");
            string readMe = "# SCBot\r\n\r\n";
            readMe += "A bot to suMMarize the [Snap Chat Political Ads Library](https://www.snap.com/en-US/political-ads).\r\n\r\n";
            readMe += "Source and summarized data in CSV format: [/SCData](https://github.com/MassMove/SCBot/tree/master/SCData).\r\n\r\n";
            readMe += "Last run: " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".\r\n\r\n";

            for (int year = DateTime.Now.Year; year >= 2018; year--)
            {
                Console.WriteLine("\r\n" + year + " summary");

                var dataFile = $"../../../../SCData/{year}.csv";
                var dataSummaryFile = $"../../../../SCData/{year}_suMMarized.csv";
                var url = $"https://storage.googleapis.com/ad-manager-political-ads-dump/political/{year}/PoliticalAds.zip";

                var campaignFileParser = new CampaignFileParser();
                var campaignSummaryWriter = new CampaignFileWriter();

                campaignFileParser.Download(dataFile, url);

                var campaigns = campaignFileParser.Parse(dataFile);
                campaigns.OrderByDescending(c => c.spend);
                campaignSummaryWriter.Write(dataSummaryFile, campaigns);
                campaignSummaryWriter.WriteReadMeYear(campaigns, year, dataFile);
                readMe = campaignSummaryWriter.WriteReadMe(campaigns, readMe, year);
            }
        }
    }
}
