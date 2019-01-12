namespace HaemophilusWeb.Models
{
    public interface ISendingReference<out TSending, TPatient>
        where TPatient : PatientBase
        where TSending : SendingBase<TPatient>
    {
        TSending Sending { get; }
    }
}