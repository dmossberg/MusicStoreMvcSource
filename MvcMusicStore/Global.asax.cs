using Microsoft.ApplicationInsights;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using MvcMusicStore.Logging;
using MvcMusicStore.Proxy;
using StackExchange.Profiling;
using System;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MvcMusicStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private ObservableEventListener listenerAzure, listenerConsole;
        TelemetryClient telemetry;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            EnableLoggingListener();
            MusicStoreEventSource.Log.Startup(Environment.MachineName);
        }

        private void EnableLoggingListener()
        {
            telemetry = new TelemetryClient();

            string storageConnectionString = ConfigurationManager.AppSettings["AzureStorageAccount"];
            listenerAzure = new ObservableEventListener();
            listenerAzure.LogToWindowsAzureTable("MvcMusicStore", storageConnectionString);
            listenerAzure.EnableEvents(MusicStoreEventSource.Log, EventLevel.LogAlways, Keywords.All);
            
            listenerConsole = new ObservableEventListener();
            listenerConsole.LogToConsole();
            listenerConsole.EnableEvents(MusicStoreEventSource.Log, EventLevel.LogAlways, Keywords.All);
        }

        protected void Application_End()
        {
            MusicStoreEventSource.Log.ShutDown(Environment.MachineName);
            DisableLoggingListener();
        }

        private void DisableLoggingListener()
        {
            //listener.DisableEvents(AuditEvent.Log);
            listenerAzure.Dispose();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            telemetry.TrackException(exception);

            while (exception.InnerException != null)
                exception = exception.InnerException;

            string exceptionType = exception.GetType().ToString();
            string exceptionMessage = exception.Message;
            string stackTrace = exception.StackTrace;

            switch( exceptionType )
            {
                case "System.Data.SqlClient.SqlException" :
                    MusicStoreEventSource.Log.DatabaseError(exceptionType, exceptionMessage, stackTrace);
                    break;
                case "MvcMusicStore.Proxy.ServiceCallException":
                    string serviceUrl = ((ServiceCallException)exception).ServiceUrl;
                    MusicStoreEventSource.Log.ServiceCallError(exceptionType, exceptionMessage, stackTrace, serviceUrl);
                    break;
                default:
                    MusicStoreEventSource.Log.UnhandledException(exceptionType, exceptionMessage, stackTrace);
                    break;
            }
            //Server.ClearError();
        }

        //Enable MiniProfiler

        //protected void Application_BeginRequest()
        //{
        //    if (Request.IsLocal)
        //    {
        //        MiniProfiler.Start();
        //    }
        //}

        //protected void Application_EndRequest()
        //{
        //    MiniProfiler.Stop();
        //}
    }
}