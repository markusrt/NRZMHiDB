using HaemophilusWeb.Models;
using TestDataGenerator;

namespace HaemophilusWeb.TestUtils
{
    public static class MockData
    {
        private const int EntityCount = 11;
        public const int FirstId = 1;
        public const int SecondId = 2;
        public const int ThirdId = 3;

        private static readonly Catalog Catalog = new Catalog();

        public static void CreateMockData(ApplicationDbContextMock dbContext)
        {
            for (var i = FirstId; i < EntityCount; i++)
            {
                var patient = Catalog.CreateInstance<Patient>();
                patient.PatientId = i;
                dbContext.PatientDbSet.Add(patient);

                var sender = Catalog.CreateInstance<Sender>();
                sender.SenderId = i;
                dbContext.Senders.Add(sender);

                var sending = Catalog.CreateInstance<Sending>();
                sending.PatientId = i;
                sending.SenderId = i;
                sending.SendingId = i;
                sending.Isolate = null;
                dbContext.Sendings.Add(sending);
            }
        }

        public static T CreateInstance<T>()
        {
            return Catalog.CreateInstance<T>();
        }
    }
}