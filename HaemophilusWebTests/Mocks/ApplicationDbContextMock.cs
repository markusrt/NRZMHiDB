﻿using System;
using System.Data.Entity;
using System.Linq;
using HaemophilusWeb.Mocks;

namespace HaemophilusWeb.Models
{
    public class ApplicationDbContextMock : IApplicationDbContext
    {
        public readonly InMemoryDbSet<Patient> PatientDbSet = new InMemoryDbSet<Patient>(true);

        public readonly InMemoryDbSet<Sender> SenderDbSet = new InMemoryDbSet<Sender>(true)
        {
            FindFunction = (senders, objects) => senders.Single(s => s.SenderId == (int) objects[0])
        };

        public readonly InMemoryDbSet<Sending> SendingDbSet = new MockDbSet<Sending>(true);
        public readonly InMemoryDbSet<Isolate> IsolatesDbSet = new InMemoryDbSet<Isolate>(true);
        public readonly InMemoryDbSet<County> CountiesDbSet = new InMemoryDbSet<County>(true);
        public readonly InMemoryDbSet<HealthOffice> HealthOfficesDbSet = new InMemoryDbSet<HealthOffice>(true);
        public readonly InMemoryDbSet<EpsilometerTest> EpsilometerTestsDbSet = new InMemoryDbSet<EpsilometerTest>(true);
        public readonly InMemoryDbSet<Meningo.MeningoPatient> MeningoPatientsDbSet = new InMemoryDbSet<Meningo.MeningoPatient>(true);

        public readonly InMemoryDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpointsDbSet =
            new InMemoryDbSet<EucastClinicalBreakpoint>(true);

        public IDbSet<Sender> Senders
        {
            get { return SenderDbSet; }
        }

        public IDbSet<Patient> Patients
        {
            get { return PatientDbSet; }
        }

        public IDbSet<Sending> Sendings
        {
            get { return SendingDbSet; }
        }

        public IDbSet<Isolate> Isolates
        {
            get { return IsolatesDbSet; }
        }

        public IDbSet<County> Counties
        {
            get { return CountiesDbSet; }
        }

        public IDbSet<HealthOffice> HealthOffices
        {
            get { return HealthOfficesDbSet; }
        }

        public IDbSet<EpsilometerTest> EpsilometerTests
        {
            get { return EpsilometerTestsDbSet; }
        }

        public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints
        {
            get { return EucastClinicalBreakpointsDbSet; }
        }

        public IDbSet<Meningo.MeningoPatient> MeningoPatients
        {
            get { return MeningoPatientsDbSet; }
        }

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