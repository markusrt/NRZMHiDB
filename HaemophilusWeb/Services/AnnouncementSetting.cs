using System;

namespace HaemophilusWeb.Services
{
    public class AnnouncementSetting
    {
        public string Text { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool IsActiveOn(DateTime date)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return false;
            }

            var day = date.Date;
            if (Start.HasValue && day < Start.Value.Date)
            {
                return false;
            }

            if (End.HasValue && day > End.Value.Date)
            {
                return false;
            }

            return true;
        }
    }
}
