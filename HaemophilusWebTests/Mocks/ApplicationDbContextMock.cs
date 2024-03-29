﻿using System;
using System.Data.Entity;
using System.Linq;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Models
{
    public class ApplicationDbContextMock : IApplicationDbContext
    {
        public readonly InMemoryDbSet<Patient> PatientDbSet = new InMemoryDbSet<Patient>(true);

        public readonly InMemoryDbSet<Sender> SenderDbSet = new InMemoryDbSet<Sender>(true)
        {
            FindFunction = (senders, objects) => senders.First(s => s.SenderId == (int) objects[0])
        };

        public readonly InMemoryDbSet<Sending> SendingDbSet = new MockDbSet<Sending>(true);
        public readonly InMemoryDbSet<Isolate> IsolatesDbSet = new InMemoryDbSet<Isolate>(true);
        public readonly InMemoryDbSet<County> CountiesDbSet = new InMemoryDbSet<County>(true);
        public readonly InMemoryDbSet<HealthOffice> HealthOfficesDbSet = new InMemoryDbSet<HealthOffice>(true);
        public readonly InMemoryDbSet<EpsilometerTest> EpsilometerTestsDbSet = new InMemoryDbSet<EpsilometerTest>(true);
        public readonly InMemoryDbSet<MeningoPatient> MeningoPatientsDbSet = new InMemoryDbSet<MeningoPatient>(true);
        public readonly InMemoryDbSet<MeningoSending> MeningoSendingsDbSet = new InMemoryDbSet<MeningoSending>(true);
        public readonly InMemoryDbSet<MeningoIsolate> MeningoIsolatesDbSet = new InMemoryDbSet<MeningoIsolate>(true);

        public readonly InMemoryDbSet<NeisseriaPubMlstIsolate> NeisseriaPubMlstIsolatesDbSet = new InMemoryDbSet<NeisseriaPubMlstIsolate>(true)
        {
            FindFunction = (pubmlstIsolates, objects) => pubmlstIsolates.Single(s => s.NeisseriaPubMlstIsolateId == (int) objects[0])
        };

        public readonly InMemoryDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpointsDbSet =
            new InMemoryDbSet<EucastClinicalBreakpoint>(true);

        public IDbSet<Sender> Senders => SenderDbSet;

        public IDbSet<Patient> Patients => PatientDbSet;

        public IDbSet<Sending> Sendings => SendingDbSet;

        public IDbSet<Isolate> Isolates => IsolatesDbSet;

        public IDbSet<County> Counties => CountiesDbSet;

        public IDbSet<HealthOffice> HealthOffices => HealthOfficesDbSet;

        public IDbSet<EpsilometerTest> EpsilometerTests => EpsilometerTestsDbSet;

        public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints => EucastClinicalBreakpointsDbSet;

        public IDbSet<MeningoPatient> MeningoPatients => MeningoPatientsDbSet;

        public IDbSet<MeningoSending> MeningoSendings => MeningoSendingsDbSet;

        public IDbSet<MeningoIsolate> MeningoIsolates => MeningoIsolatesDbSet;

        public IDbSet<NeisseriaPubMlstIsolate> NeisseriaPubMlstIsolates => NeisseriaPubMlstIsolatesDbSet;

        public void MarkAsModified<TEntity>(TEntity entity) where TEntity : class
        {
        }

        public int SaveChanges()
        {
            return 0;
        }

        public void WrapInTransaction(Action action)
        {
            action();
        }

        public void PerformWithoutSaveValidation(Action action)
        {
            action();
        }

        public void Dispose()
        {
        }
    }

    public class MockDbSet<T> : InMemoryDbSet<Sending>
    {
        public MockDbSet(bool clearDownExistingData) : base(clearDownExistingData)
        {
        }

        public override Sending Find(params object[] keyValues)
        {
            return this.FirstOrDefault(s => keyValues.Contains(s.SendingId));
        }
    }
}