using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Messaging;



namespace CompresorLZW_WindowsService
{
        public partial class Service1 : ServiceBase
        {
            //private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
            //private Thread _thread;
            private System.Timers.Timer _timer;
            private System.ComponentModel.IContainer components1;
            private System.Diagnostics.EventLog eventLog1;

            public void startMethod()
            {
               OnStart(null);
            }
            public Service1()
            {
                InitializeComponent();
                eventLog1 = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("MySource"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "MySource", "MyNewLog");
                }
                eventLog1.Source = "MySource";
                eventLog1.Log = "MyNewLog";
            }

            protected override void OnStart(string[] args)
            {
                eventLog1.WriteEntry("In OnStart");
                //try {
                    _timer = new System.Timers.Timer();
                    _timer.Interval = 5000; // 5 seconds
                    _timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                    _timer.Start();
                //}
                //catch (Exception e)
                //{
                //    string sSource = "ComponentAtrapador";
                //    string sLog = e.Message;
                //    string sEvent = "Error Event";
                //    if (!EventLog.SourceExists(sSource))
                //        EventLog.CreateEventSource(sSource, sLog);

                //    EventLog.WriteEntry(sSource, sEvent);
                //}
            }

            protected override void OnStop()
            {
                _timer.Stop();
            }

            public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
            {
                //int contador=0;
                //string nombreArchivo = "archivoMensaje";
                //while (!_shutdownEvent.WaitOne(0))
                //{
                        MessageQueue messageQueue = new MessageQueue(@".\Private$\LZWqueue");
                        System.Messaging.Message[] messages = messageQueue.GetAllMessages();

                        System.Messaging.Message m = new System.Messaging.Message();
                        //messageQueue.Purge();
                        foreach (System.Messaging.Message message in messages)
                        {

                            message.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                            string text = message.Body.ToString();


                            string[] args1 = text.Split(new char[] { ' ' }, 2);
                            LZWcontroller.Main(args1);

                            //System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + nombreArchivo + ".txt", text);
                            ////Properties.Settings.Default.SettingNumero++;
                            ////Properties.Settings.Default.Save();
                            ////Do something with the message.
                            ////contador++;
                        }
                        // after all processing, delete all the messages
                        messageQueue.Purge();
                
                    // Replace the Sleep() call with the work you need to do
                    //Thread.Sleep(5000);
                //}

            }
        }
    }
