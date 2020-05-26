using HaemophilusWeb.Models;
using NodaTime;

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
                var birthday = LocalDate.FromDateTime(isolate.Sending.Patient.BirthDate.Value);
                var samplingOrReceivingDate = LocalDate.FromDateTime(isolate.Sending.SamplingDate ?? isolate.Sending.ReceivingDate);
                ageAtSampling =  Period.Between(birthday, samplingOrReceivingDate, PeriodUnits.Years).Years;
            }
            return ageAtSampling;
        }

        /// <summary>
        ///     Calculates the patients month age at sampling
        /// </summary>
        /// <returns>Age of patient at sampling if sampling date is known, age at receiving date otherwise</returns>
        public static int PatientMonthAgeAtSampling<TSending, TPatient>(this ISendingReference<TSending, TPatient> isolate)
            where TPatient : PatientBase
            where TSending : SendingBase<TPatient>
        {
            var monthAgeAtSampling = 0;
            if (isolate.Sending.Patient.BirthDate.HasValue)
            {
                var birthday = LocalDate.FromDateTime(isolate.Sending.Patient.BirthDate.Value);
                var samplingOrReceivingDate = LocalDate.FromDateTime(isolate.Sending.SamplingDate ?? isolate.Sending.ReceivingDate);
                monthAgeAtSampling =  Period.Between(birthday, samplingOrReceivingDate, PeriodUnits.Months).Months;
            }
            return monthAgeAtSampling;
        }
    }
}