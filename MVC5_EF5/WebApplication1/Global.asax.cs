using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Clutch.Diagnostics.EntityFramework;
using NLog;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DbTracing.Enable(
                new GenericDbTracingListener()
                    .OnFinished(c => logger.Trace("-- Command finished - time: {0}{1}{2}", 
                        c.Duration, 
                        Environment.NewLine, 
                        c.Command.ToTraceString()))
                    .OnFailed(c => logger.Trace("-- Command failed - time: {0}{1}{2}", 
                        c.Duration, 
                        Environment.NewLine, 
                        c.Command.ToTraceString()))
            );

        }
    }
}
