using HaemophilusWeb.Models;

namespace HaemophilusWeb.Domain
{
    public static class IsolateExtensions
    {
        /// <summary>
        ///     Calculates the patients age at sampling at sampling
        /// </summary>
        /// <returns>Age of patient at sampling if sampling date is known, age at receiving date otherwise</returns>
        public static int PatientAgeAtSampling<TSending, TPatient>(this ISendingReference<TSending, TPatient> isolate)
            where TPatient : PatientBase
            where TSending : SendingBase<TPatient>
        {
            var ageAtSampling = 0;
            if (isolate.Sending.Patient.BirthDate.HasValue)
            {
                var birthday = isolate.Sending.Patient.BirthDate.Value;
                var samplingOrReceivingDate = isolate.Sending.SamplingDate ?? isolate.Sending.ReceivingDate;
                ageAtSampling = samplingOrReceivingDate.Year - birthday.Year;
                var birthdayIsAfterSamplingOrReceivingDate = birthday > samplingOrReceivingDate.AddYears(-ageAtSampling);
                if (birthdayIsAfterSamplingOrReceivingDate)
                {
                    ageAtSampling--;
                }
            }
            return ageAtSampling;
        }
    }
}