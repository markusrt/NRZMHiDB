﻿using System;
using System.Linq;
using HaemophilusWeb.Models;
using TestDataGenerator;

namespace HaemophilusWeb.TestUtils
{
    public static class MockData
    {
        private static readonly Random Random = new Random();
        private const int EntityCount = 11;
        public const int FirstId = 1;
        public const int SecondId = 2;
        public const int ThirdId = 3;

        static MockData()
        {
            Catalog.RegisterPostCreationHandler(new PatientPostCreationHandler());
            Catalog.RegisterPostCreationHandler(new SendingPostCreationHandler());
        }

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

        private class PatientPostCreationHandler : IPostCreationHandler
        {
            public void ApplyPostCreationRule(object instance)
            {
                if (instance is Patient patient)
                {
                    patient.Initials = $"{RandomLetter()}.{RandomLetter()}.";
                }
            }
        }

        private class SendingPostCreationHandler : IPostCreationHandler
        {
            public void ApplyPostCreationRule(object instance)
            {
                if (instance is Sending sending)
                {
                    if (sending.SamplingDate>sending.ReceivingDate)
                    {
                        var recievingDate = sending.ReceivingDate;
                        sending.ReceivingDate = sending.SamplingDate.Value;
                        sending.SamplingDate = recievingDate;

                    }
                }
            }
        }

        private static string RandomLetter()
        {
            var text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var index = Random.Next(text.Length);
            return text[index].ToString();
        }
    }
}