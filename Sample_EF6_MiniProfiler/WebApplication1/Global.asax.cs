using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private bool _enableProfiling = false;
        public bool EnableProfiling
        {
            get
            {
                if (!this._enableProfiling)
                {
                    var enableProfilingValue =
                        ConfigurationManager.AppSettings["enable_profiling"] == null
                            ? "false"
                            : ConfigurationManager.AppSettings["enable_profiling"].ToString();

                    if (!bool.TryParse(enableProfilingValue, out this._enableProfiling))
                    {
                        this._enableProfiling = false;
                    }
                }
                return this._enableProfiling;
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MiniProfilerEF6.Initialize();
        }

        protected void Application_BeginRequest()
        {
            if (Request.IsLocal)
            {
                MiniProfiler.Start();
            }
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }

    }
}
