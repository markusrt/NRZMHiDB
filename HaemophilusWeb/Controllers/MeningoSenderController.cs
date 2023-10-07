using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class MeningoSenderController : SenderControllerBase<MeningoPatient,MeningoSending>
    {
        private readonly IApplicationDbContext _db;

        public MeningoSenderController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoSenderController(IApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        protected override DatabaseType DatabaseType => DatabaseType.Meningococci;

        protected override IQueryable<MeningoSending> QuerySendings()
        {
            return _db.MeningoSendings.Include(s =>s.Isolate);
        }
    }
}