namespace HaemophilusWeb.Tools
{
    public interface IDuplicatePatientDetectionColumns
    {
        string Initials { get; }
        string DateOfBirth { get; }
        string PostalCode { get; }
        string ReceivedDate { get; }
        string Serotype { get; }
        string Sex { get; }
        string Source { get; }
        string BetaLactamase { get; }
        string AmxSir { get; }
        string StemNumber { get; }
    }
}