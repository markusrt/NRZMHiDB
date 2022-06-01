using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
using NLog;

namespace HaemophilusWeb.Services
{
    public class PubMlstMatcher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IApplicationDbContext _db;
        private readonly PubMlstService[] _pubMlstServices;

        public PubMlstMatcher(IApplicationDbContext db, params PubMlstService[] pubMlstServices)
        {
            _db = db;
            _pubMlstServices = pubMlstServices;
        }

        public List<PubMlstMatchInfo> Match(FromToQuery fromTo)
        {
            var matches = new List<PubMlstMatchInfo>();

            var matchingIsolates = _db.MeningoIsolates.Include(i => i.Sending).Where(i =>
                i.Sending.SamplingDate == null && i.Sending.ReceivingDate >= fromTo.From && i.Sending.ReceivingDate <= fromTo.To
                || i.Sending.SamplingDate >= fromTo.From && i.Sending.SamplingDate <= fromTo.To).ToList();

            foreach (var isolate in matchingIsolates)
            {
                var pubMlstMatchInfo = new PubMlstMatchInfo
                {
                    IsolateId = isolate.MeningoIsolateId,
                    StemNumber = isolate.StemNumberWithPrefix,
                    LaboratoryNumber = isolate.LaboratoryNumberWithPrefix
                };
                NeisseriaPubMlstIsolate pubMlstIsolate = null;

                if (isolate.StemNumber.HasValue)
                {
                    foreach (var pubMlstService in _pubMlstServices)
                    {
                        pubMlstIsolate = pubMlstService.GetIsolateByReference(isolate.StemNumberWithPrefix);
                        if (pubMlstIsolate != null)
                        {
                            break;
                        }
                    }
                    Log.Error($"Failed to match isolate by reference {isolate.StemNumberWithPrefix}");
                }

                if (pubMlstIsolate != null)
                {
                    pubMlstIsolate = CreateOrUpdatePubMlstIsolate(isolate, pubMlstIsolate);
                    _db.MarkAsModified(isolate);
                    _db.SaveChanges();

                    pubMlstMatchInfo.NeisseriaPubMlstIsolateId = pubMlstIsolate.NeisseriaPubMlstIsolateId;
                    pubMlstMatchInfo.PubMlstId = pubMlstIsolate.PubMlstId;
                    pubMlstMatchInfo.Database = pubMlstIsolate.Database;
                }

                matches.Add(pubMlstMatchInfo);
            }
            return matches;
        }

        private NeisseriaPubMlstIsolate CreateOrUpdatePubMlstIsolate(MeningoIsolate isolate, NeisseriaPubMlstIsolate pubMlstIsolate)
        {
            //TODO fix this duplication from MeningoIsolateController and try to find a
            //mapping framework which does not mess with EntityFramework context as automapper
            var pubMlstIsolateFromDatabase =
                _db.NeisseriaPubMlstIsolates.SingleOrDefault(n =>
                    n.PubMlstId == pubMlstIsolate.PubMlstId);
            if (pubMlstIsolateFromDatabase == null)
            {
                pubMlstIsolateFromDatabase = pubMlstIsolate;
                _db.NeisseriaPubMlstIsolates.Add(pubMlstIsolateFromDatabase);
            }
            else
            {
                pubMlstIsolateFromDatabase.PorAVr1 = pubMlstIsolate.PorAVr1;
                pubMlstIsolateFromDatabase.PorAVr2 = pubMlstIsolate.PorAVr2;
                pubMlstIsolateFromDatabase.FetAVr = pubMlstIsolate.FetAVr;
                pubMlstIsolateFromDatabase.PorB = pubMlstIsolate.PorB;
                pubMlstIsolateFromDatabase.Fhbp = pubMlstIsolate.Fhbp;
                pubMlstIsolateFromDatabase.Nhba = pubMlstIsolate.Nhba;
                pubMlstIsolateFromDatabase.NadA = pubMlstIsolate.NadA;
                pubMlstIsolateFromDatabase.PenA = pubMlstIsolate.PenA;
                pubMlstIsolateFromDatabase.GyrA = pubMlstIsolate.GyrA;
                pubMlstIsolateFromDatabase.ParC = pubMlstIsolate.ParC;
                pubMlstIsolateFromDatabase.ParE = pubMlstIsolate.ParE;
                pubMlstIsolateFromDatabase.RpoB = pubMlstIsolate.RpoB;
                pubMlstIsolateFromDatabase.RplF = pubMlstIsolate.RplF;
                pubMlstIsolateFromDatabase.SequenceType = pubMlstIsolate.SequenceType;
                pubMlstIsolateFromDatabase.ClonalComplex = pubMlstIsolate.ClonalComplex;
                pubMlstIsolateFromDatabase.BexseroReactivity = pubMlstIsolate.BexseroReactivity;
                pubMlstIsolateFromDatabase.TrumenbaReactivity = pubMlstIsolate.TrumenbaReactivity;
            }

            isolate.NeisseriaPubMlstIsolate = pubMlstIsolateFromDatabase;
            return pubMlstIsolateFromDatabase;
        }
    }
}