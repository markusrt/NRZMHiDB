using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class SenderController : SenderControllerBase<Patient,Sending>
    {
        private readonly IApplicationDbContext _db;

        public SenderController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public SenderController(IApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        protected override DatabaseType DatabaseType => DatabaseType.Haemophilus;

        protected override IQueryable<Sending> QuerySendings()
        {
            return _db.Sendings.Include(s =>s.Isolate);
        }
    }
}