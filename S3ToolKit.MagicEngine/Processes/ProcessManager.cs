/*
    Copyright 2012, Grant Hess

    This file is part of S3ToolKit.MagicEngine.

    S3ToolKit.Utils is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with CC Magic.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using S3ToolKit.Utils.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace S3ToolKit.MagicEngine.Processes
{
    // Singleton using Lazy<T> from http://geekswithblogs.net/BlackRabbitCoder/archive/2010/05/19/c-system.lazylttgt-and-the-singleton-design-pattern.aspx
    public class ProcessManager : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton
        private static readonly Lazy<ProcessManager> _instance = new Lazy<ProcessManager>(() => new ProcessManager());

        public static ProcessManager Instance { get { return _instance.Value; } }
        #endregion

        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // ActiveSharp property change notification implementation
        // use this layout to set properties:
        //     public int Foo
        //     {
        //       get { return _foo; }
        //       set { SetValue(ref _foo, value); }   // assigns value and does prop change notification, all in one line
        //     }
        protected void SetValue<T>(ref T field, T value)
        {
            field = value;   //Actually assign the new value
            PropertyInfo changedProperty = ActiveSharp.PropertyMapping.PropertyMap.GetProperty(this, ref field);

            OnPropertyChanged(changedProperty.Name);
        }
        #endregion

        #region Fields        
        private ConcurrentQueue<IMagicProcess> WorkQueue;
        private List<IMagicProcess> _ActiveTaskList;
        private readonly object _ActiveTaskListLock = new object();
        private List<Task> ThreadPool;
        private bool _IsRunning;
        private bool _IsStopping;
        private int _ThreadMaxCount;
        
        #endregion

        #region Properties
        public int ThreadMaxCount
        {
            get { return _ThreadMaxCount; }
            set { SetValue(ref _ThreadMaxCount, value); }
        }
        public bool IsRunning 
             {
               get { return _IsRunning; }
               set { SetValue(ref _IsRunning, value); }   // assigns value and does prop change notification, all in one line
             }
        public bool IsStopping
        {
            get { return _IsStopping; }
            set { SetValue(ref _IsStopping, value); }   // assigns value and does prop change notification, all in one line
        }
        #endregion

        #region Constructors
        private ProcessManager()
        {
            log.Debug("Starting Process Manager");
            ThreadMaxCount = Environment.ProcessorCount * 4;
            if (ThreadMaxCount < 4)
                ThreadMaxCount = 4;
            log.Debug(string.Format("Found {0} CPU Cores, starting {1} Threads",Environment.ProcessorCount, ThreadMaxCount));

            // Configure housekeeping stuff
            _ActiveTaskList = new List<IMagicProcess>();
            WorkQueue = new ConcurrentQueue<IMagicProcess>();
            IsRunning = false;

            // Create and start the task pool
            ThreadPool = new List<Task>();
            
            for (int i = 0; i < ThreadMaxCount; i++)
            {
                Task SchedulerTask;
                SchedulerTask = new Task(() => ProcessQueueRunner(),
                        TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
                SchedulerTask.Start();  // Starts the queue runner, but doesn't enable task execution yet            
                ThreadPool.Add(SchedulerTask);
                log.Debug(string.Format("Thread Created: ID {0}", SchedulerTask.Id));
            }
        }
        #endregion

        #region High Level Process Management
        public void StartProcessQueue()
        {
            IsRunning = true;  // queue runner checks this and starts/stops child threads as needed            
            log.Debug("Enable threads");
        }

        public void StopProcessQueue()
        {
            IsRunning = false;
            log.Debug("Disable threads");
        }

        private void ProcessQueueRunner()
        {
            log.Debug(string.Format("Thread {0} starting", System.Threading.Thread.CurrentThread.ManagedThreadId));
            while (!IsStopping)
            {
                IMagicProcess NextTask;
                if (WorkQueue.TryDequeue(out NextTask))
                {
                    lock (_ActiveTaskListLock)
                    {
                        _ActiveTaskList.Add(NextTask);
                    }
                    // DO processing here

                    lock (_ActiveTaskListLock)
                    {
                        _ActiveTaskList.Remove(NextTask);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(250);
                }

            }
            IsRunning = false;
            log.Debug(string.Format("Thread {0} stopping", System.Threading.Thread.CurrentThread.ManagedThreadId));
        } 
        #endregion
    }
}
