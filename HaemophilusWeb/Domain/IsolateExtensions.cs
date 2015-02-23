using HaemophilusWeb.Models;

namespace HaemophilusWeb.Domain
{
    public static class IsolateExtensions
    {
        public static int PatientAge(this Isolate isolate)
        {
            var age = 0;
            if (isolate.Sending.Patient.BirthDate.HasValue)
            {
                var birthday = isolate.Sending.Patient.BirthDate.Value;
                var samplingDate = isolate.Sending.SamplingDate ?? isolate.Sending.ReceivingDate;
                var ageAtSampling = samplingDate.Year - birthday.Year;
                if (birthday > samplingDate.AddYears(-ageAtSampling))
                {
                    ageAtSampling--;
                }
                age = ageAtSampling;
            }
            return age;
        }
    }
}