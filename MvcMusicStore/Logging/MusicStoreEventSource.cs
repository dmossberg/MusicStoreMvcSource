using System;
using System.Diagnostics.Tracing;

namespace MvcMusicStore.Logging
{
    public class MusicStoreEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        public class Tasks
        {
            public const EventTask Page = (EventTask)1;
            public const EventTask DBQuery = (EventTask)2;
        }

        //create a static field instance that provides access to an instance of this class
        private static readonly Lazy<MusicStoreEventSource> Instance
          = new Lazy<MusicStoreEventSource>(() => new MusicStoreEventSource());
         //create a static property called Log that returns the current value 
        //of the Instance field of the event source. This value is 
        //determined by the custom event methods called in the Customerservices application.
        private MusicStoreEventSource() { }
        
        public static MusicStoreEventSource Log
        {
            get { return Instance.Value; }
        }

        [Event(1, Message = "Application Failure: {0}",
        Level = EventLevel.Critical, Keywords = Keywords.Diagnostic)]
        internal void Failure(string message)
        {
            this.WriteEvent(1, message);
        }

        [Event(2, Message = "Starting up in: {0}", Keywords = Keywords.Perf,
        Level = EventLevel.Informational)]
        internal void Startup(string servername)
        {
            this.WriteEvent(2, servername);
        }

        [Event(3, Message = "Shutting down in: {0}", Keywords = Keywords.Perf,
        Level = EventLevel.Informational)]
        internal void ShutDown(string servername)
        {
            this.WriteEvent(3, servername);
        }

        [Event(4, Message = "Loading page {1} activityID={0}",
        Opcode = EventOpcode.Start,
        Task = Tasks.Page, Keywords = Keywords.Page,
        Level = EventLevel.Informational)]
        internal void PageStart(int ID, string url)
        {
            this.WriteEvent(4, ID, url);
        }

        

        [Event(100, Message = "Unhandled exception: {0}",
        Task = Tasks.Page, Keywords = Keywords.Diagnostic, Level = EventLevel.Error)]
        internal void UnhandledException(string type, string message, string stackTrace)
        {
            this.WriteEvent(100, type, message, stackTrace);
        }

        [Event(101, Message = "Database access error", Keywords = Keywords.DataBase, Level = EventLevel.Error)]
        internal void DatabaseError(string type, string message, string stackTrace)
        {
            this.WriteEvent(101, type, message, stackTrace);
        }

        [Event(102, Message = "Outbound service call error", Keywords = Keywords.Diagnostic, Level = EventLevel.Error)]
        internal void ServiceCallError(string type, string message, string stackTrace, string url)
        {
            this.WriteEvent(102, type, message, stackTrace, url);
        }
    }
}