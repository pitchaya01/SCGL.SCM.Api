using Autofac;
using Lazarus.Common.Authentication;
using Lazarus.Common.DI;
using Lazarus.Common.Domain.Seedwork;
using Lazarus.Common.Interface;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Lazarus.Common.Utilities
{
    public class AppConfigUtilities
    {
        public static IConfiguration _configuration { get; set; }
        public static CultureInfo CreateCulture(string langCode)
        {
            switch (langCode)
            {
                case "th":
                {
                    return CultureInfo.CreateSpecificCulture("th-TH");
                }

                default:
                {
                    return CultureInfo.CreateSpecificCulture("en-GB");
                }
            }
        }
        public static string GetDomain()
        {
            var user = DomainEvents._Container.Resolve<IUserService>();
            var context = user.GetHttpContext();
            return context.Request.Host.Host;


        }

        public static T GetAppConfig<T>(string key)
        {

         
            var value = AppConfigUtilities._configuration.GetSection("AppSettings:"+key).Value; 
            if (string.IsNullOrEmpty(value)) throw new Exception(string.Format("Config {0} value not found", key));
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                var result = (T)converter.ConvertFromString(null,
                    CultureInfo.InvariantCulture, value);
                return result;

            }
            catch (Exception)
            {
                throw new Exception(string.Format("Invalid Convert type Value:{0}", key));
            }


        }

        public static string FirstLetterToUpper(string sortField)
        {
            if (sortField == null)
                return null;

            if (sortField.Length > 1)
                return char.ToUpper(sortField[0]) + sortField.Substring(1);

            return sortField.ToUpper();
        }
        public class SearchUtil
        {
            public static IQueryable<T> SkipData<T>(IQueryable<T> searchResult, int? page, int? pageSize, int? total = 0)
            {
                if (page == null || page == 0) page = 1;
                if (pageSize == null || pageSize == 0) pageSize = 10;

                if (total != null && total != 0)
                {
                    var maxpage = ((total - 1) / pageSize) + 1;
                    if (page > maxpage)
                    {
                        page = 1;
                    }
                }

                int skip = (int)((page - 1) * pageSize);
                searchResult = searchResult.Skip(skip).Take((int)pageSize);

                return searchResult;
            }

            public static List<T> SkipListData<T>(List<T> searchResult, int? page, int? pageSize, int? total = 0)
            {
                if (page == null || page == 0) page = 1;
                if (pageSize == null || pageSize == 0) pageSize = 10;

                if (total != null && total != 0)
                {
                    var maxpage = ((total - 1) / pageSize) + 1;
                    if (page > maxpage)
                    {
                        page = 1;
                    }
                }

                int skip = (int)((page - 1) * pageSize);
                searchResult = searchResult.Skip(skip).Take((int)pageSize).ToList();

                return searchResult;
            }

            public static IQueryable<T> SortData<T>(IQueryable<T> searchResult, int? sortOrder, string sortField)
            {
                if (sortOrder == 1)
                    searchResult = searchResult.OrderBy(i => i.GetType().GetProperty(sortField).GetValue(i, null));
                else if (sortOrder == -1)
                    searchResult = searchResult.OrderByDescending(i => i.GetType().GetProperty(sortField).GetValue(i, null));

                return searchResult;
            }

            public static List<T> SortListData<T>(List<T> searchResult, int? sortOrder, string sortField)
            {
                if (sortOrder == 1)
                    searchResult = searchResult.OrderBy(i => i.GetType().GetProperty(sortField).GetValue(i, null)).ToList();
                else if (sortOrder == -1)
                    searchResult = searchResult.OrderByDescending(i => i.GetType().GetProperty(sortField).GetValue(i, null)).ToList();

                return searchResult;
            }

            public static IQueryable<T> SortDataByExpression<T>(IQueryable<T> searchResult, int? sortOrder, string sortField)
            {
                if (!string.IsNullOrEmpty(sortField))
                {
                    var type = typeof(T);
                    var prop = type.GetProperty(sortField);
                    if (prop != null)
                    {
                        var param = Expression.Parameter(type);
                        var expr = Expression.Lambda<Func<T, object>>(
                            Expression.Convert(Expression.Property(param, prop), typeof(object)),
                            param
                        );
                        if (sortOrder == -1)
                            searchResult = searchResult.OrderByDescending(expr);
                        else
                            searchResult = searchResult.OrderBy(expr);
                    }
                }
                return searchResult;
            }
        }
    }
}
