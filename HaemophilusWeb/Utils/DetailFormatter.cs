using System;
using System.Collections.Generic;
using System.Globalization;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Utils
{
    public static class DetailFormatter
    {
        public static string ToDetail(this Sender sender)
        {
            if (sender == null)
            {
                return "Kein Einsender";
            }
            return string.IsNullOrEmpty(sender.Department)
                ? $"#{sender.SenderId:000}: {sender.Name} - {sender.PostalCode} {sender.City}"
                : $"#{sender.SenderId:000}: {sender.Name} ({sender.Department}) - {sender.PostalCode} {sender.City}";
        }
    }
}