using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_X_Synchronization_Server
{
    class SynchronizationScheduler
    {
        public static SynchronizationScheduler instance;
        private static int SecondsBetweenSynchronizations = -1;

        #region Locking
        private static readonly object lockObj = new object();
        #endregion

        private DateTime TimeStarted = default(DateTime);
        private DateTime NextSynchronization = default(DateTime);
        private bool Running = false;
        private bool SyncNow = false;

        private Thread SyncThread;
        public int SecondsUntilSynchronization = 0;

        private int LineNumber = -1;

        public SynchronizationScheduler()
        {
            instance = this;
        }

        public void LoadSynchronizationSettings()
        {
            LineNumber = Log.log("Loading synchronization settings..", Log.LogType.SYNC);
            SecondsBetweenSynchronizations = Database.instance.RequestSynchronizationTime();
            if (SecondsBetweenSynchronizations != -1)
            {
                Log.log(LineNumber, "Loaded synchronization settings, Seconds between synchronizations: " + SecondsBetweenSynchronizations.ToString(), Log.LogType.SUCCESS);
            }
            LineNumber = -1;
        }

        public void Start()
        {
            TimeStarted = DateTime.Now;
            NextSynchronization = TimeStarted.AddSeconds(SecondsBetweenSynchronizations);
            Log.log("Starting synchronization thread..", Log.LogType.SYNC);
            Running = true;
            SyncThread = new Thread(new ThreadStart(StartSynchronization));
            SyncThread.Start();
        }
        public void Stop()
        {
            Log.log("Synchronization stopped.", Log.LogType.SYNC);
            Running = false;
        }

        private void StartSynchronization()
        {
            SecondsUntilSynchronization = (int)((NextSynchronization - DateTime.Now).TotalSeconds);
            int LastSeconds = 0;
            int LineNumber = Log.log("Starting synchronization of data..", Log.LogType.SYNC);
            while (Running)
            {
                lock (lockObj)
                {
                    SecondsUntilSynchronization = (int)((NextSynchronization - DateTime.Now).TotalSeconds);
                    if (SecondsUntilSynchronization < 0 || SyncNow)
                    {
                        // Synchronize
                        Log.log(LineNumber, "Starting synchronization of data..", Log.LogType.SYNC);
                        Response r = Database.instance.Synchronize(LineNumber);
                        switch (r)
                        {
                            case Response.SUCCESSFUL:
                                Log.log(LineNumber, "Synchronization of data successful.", Log.LogType.SUCCESS);
                                break;
                            case Response.UNSUCCESSFUL:
                                Log.log(LineNumber, "Synchronization of data unsuccessful.", Log.LogType.ERROR);
                                break;
                            case Response.ERROR:
                                Log.log(LineNumber, "Synchronization of data unsuccessful, fix errors and try again.", Log.LogType.ERROR);
                                break;
                            default:
                                break;
                        }
                        NextSynchronization = DateTime.Now.AddSeconds(SecondsBetweenSynchronizations);
                        SyncNow = false;
                        //LineNumber = -1;
                    }
                    else
                    {
                        if (SecondsUntilSynchronization != LastSeconds)
                        {
                            if (LineNumber == -1)
                            {
                                LineNumber = Log.log("Synchronization of data happening in " + SecondsUntilSynchronization.ToString() + " seconds.", Log.LogType.SYNC);
                            }
                            else
                            {
                                if (SecondsUntilSynchronization == 0)
                                {
                                    Log.log(LineNumber, "Starting data synchronization..", Log.LogType.SYNC);
                                }
                                else
                                {
                                    Log.log(LineNumber, "Synchronization of data happening in " + SecondsUntilSynchronization.ToString() + " seconds.", Log.LogType.SYNC);
                                }
                            }
                            LastSeconds = SecondsUntilSynchronization;
                        }
                    }
                }
            }
            SyncThread.Join();
        }
        public void SynchronizeNow()
        {
            lock (lockObj)
            {
                SyncNow = true;
            }
        }
    }
}
