using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using System.Numerics;
using Lazarus.Common.Enum;
using static Lazarus.Common.APPCONSTANT;

namespace Lazarus.Common.Utilities
{
    public class Util
    {
        public static string GetExtenstionFile(string filename)
        {
            return Path.GetExtension(filename);
        }

        public static string GetOrderPrefix(SourceOrderType source, OrderType type)
        {
            var pre = "";
            switch (source)
            {
                case SourceOrderType.SAP_DSP:
                    pre = SourceOrderCode.SAP_DSP;
                    break;
                case SourceOrderType.SAP_TEP:
                    pre = SourceOrderCode.SAP_TEP;
                    break;
                case SourceOrderType.SGI:
                    pre = SourceOrderCode.SGI;
                    break;
                case SourceOrderType.COAL:
                    pre = SourceOrderCode.COAL;
                    break;
                case SourceOrderType.PFA:
                    pre = SourceOrderCode.CPAC;
                    break;
                case SourceOrderType.SSP:
                    pre = SourceOrderCode.SSP;
                    break;
            }
            switch (type)
            {
                case OrderType.SO:
                    pre += OrderTypeCode.SO;
                    break;

                case OrderType.RO:
                    pre += OrderTypeCode.RO;
                    break;

                case OrderType.STO:
                    pre += OrderTypeCode.STO;
                    break;

                case OrderType.DNO:
                    pre += OrderTypeCode.DNO;
                    break;
            }
            return pre;
        }

        public static dynamic Cast(dynamic obj, Type castTo)
        {
            try
            {

                return Convert.ChangeType(obj, castTo);
            }
            catch 
            {
                return null;
            }
        }

        public static decimal CalUnitKG(string unit, decimal weight, decimal qty)
        {
            if (qty == 0)
            {
                return 0;
            }
            unit = unit?.ToUpper() ?? "";
            var multiple = 0m;

            if (unit == "KG" || unit == "KGM")
            {
                multiple = 1m;
            }
            else if (unit == "TON" || unit == "TNE" || unit == "MT")
            {
                multiple = 1000m;
            }
            else if (unit == "G" || unit == "GRM")
            {
                multiple = 0.001m;
            }
            else if (unit == "KT")
            {
                multiple = 1000000m;
            }
            else if (unit == "LB" || unit == "LBR")
            {
                multiple = 0.4535m;
            }
            else if (unit == "OZ")
            {
                multiple = 0.0283m;
            }
            else if (unit == "MG" || unit == "MGM")
            {
                multiple = 0.000001m;
            }
            return (weight * multiple) / qty;
        }

        public static decimal IfZeroReplace(decimal? mayEmpty, decimal replace)
        {
            return (mayEmpty.HasValue && mayEmpty.Value > 0) ? mayEmpty.Value : replace;
        }

        public  static string ToJson(object obj )
        {
           var s= JsonConvert.SerializeObject(obj, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    });
            return s;
        }

        public static bool CompareSerialization(string latest, string current)
        {
            var latestBigInt = BigInteger.Parse(latest);
            var currentBigInt = BigInteger.Parse(current);
            if (latestBigInt < currentBigInt)
                return true;

            return false;
        }

		public static bool CompareSerializationINTL(string latest, string current)
		{
			var latestBigInt = BigInteger.Parse(latest.Substring(0,14));
			var currentBigInt = BigInteger.Parse(current.Substring(0,14));

			if (latestBigInt < currentBigInt)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
