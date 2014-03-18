using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class PatientSendingViewModel
    {
        public Patient Patient { get; set; }

        public Sending Sending { get; set; }
    }
}