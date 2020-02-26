using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SCBot.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToFormattedCurrencyString(this in decimal @this, string isoCurrencyCode)
        {
            var cultureInfo = GetCultureInfosByCurrencySymbol(isoCurrencyCode);

            var culture = cultureInfo.FirstOrDefault();

            if (culture is null)
            {
                return @this.ToString("C", CultureInfo.InvariantCulture);
            }

            return @this.ToFormattedCurrencyString(isoCurrencyCode, culture);
        }

        public static string ToFormattedCurrencyString(this in decimal @this, string isoCurrencyCode, CultureInfo userCulture)
        {
            try
            {
                var userCurrencyCode = new RegionInfo(userCulture.Name).ISOCurrencySymbol;

                if (userCurrencyCode == isoCurrencyCode)
                {
                    return @this.ToString("C", userCulture);
                }

                var value = string.Format("{0} {1}", isoCurrencyCode, @this.ToString("N2", userCulture));

                byte[] bytes = Encoding.Default.GetBytes(value);

                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return @this.ToString("C", CultureInfo.InvariantCulture);
            }
        }

        private static IEnumerable<CultureInfo> GetCultureInfosByCurrencySymbol(string currencySymbol)
        {
            if (currencySymbol == null)
            {
                throw new ArgumentNullException("currencySymbol");
            }

            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(x => new RegionInfo(x.LCID).ISOCurrencySymbol == currencySymbol);
        }
    }
}