using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContextMenu = System.Windows.Forms.ContextMenu;

namespace IdlingMiner
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool Closing = false;
        public MainWindow()
        {
            InitializeComponent();
            logger = new EventLog();

            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
            MyNotifyIcon.Icon = new System.Drawing.Icon(
                            "Icon_Archive.ico");
            MyNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            MyNotifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit_Form);
            MyNotifyIcon.ContextMenuStrip.Items.Add("Open menu", null, MyNotifyIcon_MouseDoubleClick);
            textBoxPool.Text = POOL;
            textBoxUser.Text = USER;
            textBox1.Text ="Stopped";
            textBox1.Background = Brushes.Red;

            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
            MyNotifyIcon.BalloonTipText = "Minimized the app ";
            MyNotifyIcon.ShowBalloonTip(400);
            MyNotifyIcon.Visible = true;
            /*PassWindows pw = new PassWindows();
            pw.ShowDialog();
            if (!pw.Access)
                this.Close();*/

        }

        private void MyNotifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            
            PassWindows pw = new PassWindows();
           pw.ShowDialog();
           if (pw.Access)
                this.WindowState = WindowState.Normal;


        }

        private void Exit_Form(object sender, EventArgs e)
        {
            
            PassWindows pw = new PassWindows();
           pw.ShowDialog();
            if (!pw.Access)
            {
                Closing = true;
                this.Close();
            }

        }

        Thread th;
        EventLog logger;
        NotifyIcon MyNotifyIcon;
        string POOL= "tcp://asia1.ethermine.org:4444", USER ="0x12343bdgf.worker";
        private void button_Click(object sender, RoutedEventArgs e)
        {
            USER = textBoxUser.Text.ToString();
            POOL = textBoxPool.Text.ToString();
            th = new Thread(toDo);
            /////logger///
            logger = new EventLog();
            if (!EventLog.SourceExists("SourceOfLogger"))
            {
                EventLog.CreateEventSource(
                    "SourceOfLogger", "MinerLogger");
            }
            ////////////////////
            logger.Source = "SourceOfLogger";
            logger.Log = "MinerLogger";
            logger.WriteEntry("Started miner service.", EventLogEntryType.Information);
            th.Start();
        }
        void toDo()
        {
            logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
            Dispatcher.Invoke(() =>
            {
                textBox1.Text = "Started Thread";
                textBox1.Background = Brushes.Yellow;
            });
            logger.WriteEntry("Starting up a programm cycle.", EventLogEntryType.Information);
            while (DateTime.Now < new DateTime(2021, 04, 07))
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
                if (ts.TotalSeconds >= 5 && !P_Cheker.isThereAProccess())
                {
                    Process p = new Process();
                    //p.StartInfo.FileName = @"D:\nb\start_service.bat";
                    p.StartInfo.FileName = @"D:\nb\nbminer.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.ErrorDialog = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    logger.WriteEntry("Setting up a miner configuration to start", EventLogEntryType.Information);
                    p.StartInfo.Arguments = "-a ethash -o "+POOL+" -u "+USER;
                    try
                    {

                        p.Start();
                        P_Cheker.PN = p.ProcessName;
                        logger.WriteEntry("Succesfully started procces of mining.", EventLogEntryType.Information);
                        logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                        Dispatcher.Invoke(() =>
                        {
                            textBox1.Text = "Mining";
                            textBox1.Background = Brushes.Green;
                        });
                    }
                    catch
                    {
                        logger.WriteEntry("Cannot start a mining proccess. No access to miner executable!", EventLogEntryType.Error);
                    }

                }
                else if (P_Cheker.isThereAProccess() && ts.TotalSeconds < 5)
                {
                    Process[] listProc = Process.GetProcesses();
                    foreach (var p in listProc)
                    {
                        if (p.ProcessName == P_Cheker.PN)
                        {
                            p.Kill();
                            logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                            Dispatcher.Invoke(() =>{
                                textBox1.Text = "Started Thread";
                                textBox1.Background = Brushes.Yellow;
                            });
                        }
                    }
                }

            }
            logger.WriteEntry("Trial Expired", EventLogEntryType.Error);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
          //  if (Closing)
           // {
             //   return;
           // }
  //          e.Cancel = true;

//            this.Activate();

        //    this.WindowState = WindowState.Minimized;
         //   return;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Process[] listProc = Process.GetProcesses();
            foreach (var p in listProc)
            {
                if (p.ProcessName == P_Cheker.PN)
                {
                    p.Kill();
                    logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app ";
                MyNotifyIcon.ShowBalloonTip(400);
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }


        private void button_ClickStop(object sender, RoutedEventArgs e)
        {
            th.Abort();
            Dispatcher.Invoke(() => {
                textBox1.Text = "Stop thread";
                textBox1.Background = Brushes.Red;
            });
            logger.WriteEntry("Stop a miner Thread", EventLogEntryType.Warning);
            Process[] listProc = Process.GetProcesses();
            foreach (var p in listProc)
            {
                if (p.ProcessName == P_Cheker.PN)
                {
                    p.Kill();
                    logger.WriteEntry("Stoping Miner cause of pressing Stop button.", EventLogEntryType.Warning);
                }
            }
        }
    }
}
