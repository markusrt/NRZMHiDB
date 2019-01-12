using HaemophilusWeb.Models;

namespace HaemophilusWeb.Domain
{
    public static class IsolateExtensions
    {
        public static int PatientAge<TSending, TPatient>(this ISendingReference<TSending, TPatient> isolate)
            where TPatient : PatientBase
            where TSending : SendingBase<TPatient>
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