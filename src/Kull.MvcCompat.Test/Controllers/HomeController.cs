using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kull.MvcCompat.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly DisposeTracker disposeTracker;
        private readonly DisposeTracker2 disposeTracker2;

        public HomeController(DisposeTracker disposeTracker, DisposeTracker2 disposeTracker2)
        {
            this.disposeTracker = disposeTracker;
            this.disposeTracker2 = disposeTracker2;
        }

        // GET: Home
        public ActionResult Index()
        {
            return Json(new
            {
                GuidScoped = disposeTracker.GetGuidString(),
                GuidTransient = disposeTracker2.GetGuidString()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}