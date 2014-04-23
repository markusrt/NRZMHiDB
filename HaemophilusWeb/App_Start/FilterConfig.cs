﻿using System.Web.Mvc;

namespace HaemophilusWeb
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ExceptionLogger());
            filters.Add(new HandleErrorAttribute());
        }
    }
}