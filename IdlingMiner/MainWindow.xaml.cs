using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Text.Json;
using ContextMenu = System.Windows.Forms.ContextMenu;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

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

            this.Visibility = Visibility.Hidden;

            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
            //MyNotifyIcon.Icon = new System.Drawing.Icon("..\\..\\icon.ico");
            MyNotifyIcon.Icon = new System.Drawing.Icon("icon.ico");
            MyNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            //MyNotifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit_Form);
            MyNotifyIcon.ContextMenuStrip.Items.Add("Open menu", null, MyNotifyIcon_Open);
            textBoxPool.Text = POOL;
            textBoxUser.Text = USER;

            textBox1.Text = "Stopped";
            textBox1.Background = Brushes.Red;
            textBox1.Foreground = Brushes.White;

            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
            MyNotifyIcon.BalloonTipText = "Minimized the app ";
            MyNotifyIcon.ShowBalloonTip(400);
            MyNotifyIcon.Visible = true;

            Settings();

        }
      

        private void MyNotifyIcon_Open(object sender, EventArgs e)
        {

            PassWindows pw = new PassWindows();
            pw.ShowDialog();
            if (pw.Access)
            {
                this.WindowState = WindowState.Normal;
                this.Visibility = Visibility.Visible;
                this.ShowInTaskbar = true;
                this.Focus();
            }


        }

        //private void Exit_Form(object sender, EventArgs e)
        //{

        //    PassWindows pw = new PassWindows();
        //    pw.ShowDialog();
        //    if (!pw.Access)
        //    {
        //        MyNotifyIcon.Visible = false;
        //        System.Windows.Application.Current.Shutdown();
        //    }

        //}

        Thread th;
        EventLog logger;
        NotifyIcon MyNotifyIcon;
        string POOL = "tcp://asia1.ethermine.org:4444", USER = "0x12343bdgf.worker";
        bool toRun = true;
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (toRun)
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
                toRun = false;
            }
            else
            {

                th.Abort();
                Dispatcher.Invoke(() =>
                {
                    textBox1.Text = "Stop thread";
                    textBox1.Background = Brushes.Red;
                    textBox1.Foreground = Brushes.White;
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
                toRun = true;
            }
        }
        void toDo()
        {
            logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
            Dispatcher.Invoke(() =>
            {
                textBox1.Text = "Started Thread";
                textBox1.Background = Brushes.Yellow;
                textBox1.Foreground = Brushes.Black;
            });
            logger.WriteEntry("Starting up a programm cycle.", EventLogEntryType.Information);
            while (DateTime.Now < new DateTime(2021, 04, 07))
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime());
                ///START
                if (ts.TotalSeconds >= 5 && !P_Cheker.isThereAProccess())
                {
                    Process p = new Process();
                    //p.StartInfo.FileName = @"D:\nb\nbminer.exe";
                    p.StartInfo.FileName = Directory.GetCurrentDirectory()+ "\\nb\\nbminer.exe";
                    //p.StartInfo.UseShellExecute = false;
                    //p.StartInfo.RedirectStandardError = true;
                    //p.StartInfo.RedirectStandardInput = true;
                    //p.StartInfo.RedirectStandardOutput = true;
                    //p.StartInfo.CreateNoWindow = true;
                    //p.StartInfo.ErrorDialog = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    logger.WriteEntry("Setting up a miner configuration to start", EventLogEntryType.Information);
                    p.StartInfo.Arguments = "-a ethash -o " + POOL + " -u " + USER;
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
                            textBox1.Foreground = Brushes.Yellow;
                        });
                    }
                    catch
                    {
                        logger.WriteEntry("Cannot start a mining proccess. No access to miner executable!", EventLogEntryType.Error);
                    }

                }
                ///STOP
                else if (P_Cheker.isThereAProccess() && ts.TotalSeconds < 5)
                {
                    Process[] listProc = Process.GetProcesses();
                    foreach (var p in listProc)
                    {
                        if (p.ProcessName == P_Cheker.PN)
                        {
                            try
                            {
                                p.Kill();
                            }
                            catch
                            {
                                Process[] listProc2 = Process.GetProcesses();
                                foreach (var p2 in listProc)
                                {
                                    if (p2.ProcessName == P_Cheker.PN)
                                    {
                                        try
                                        {
                                            p2.Kill();

                                        }
                                        catch
                                        {
                                            logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                                        }
                                        break;
                                    }
                                    logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                                }
                            }
                            logger.WriteEntry("Stoping Miner cause of input action.", EventLogEntryType.Warning);
                            Dispatcher.Invoke(() =>
                            {
                                textBox1.Text = "Started Thread";
                                textBox1.Background = Brushes.Yellow;
                                textBox1.Foreground = Brushes.Black;
                            });
                        }
                    }
                }

            }
            logger.WriteEntry("Trial Expired", EventLogEntryType.Error);
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            MyNotifyIcon.Visible = false;
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
                this.Visibility = Visibility.Hidden;
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app ";
                MyNotifyIcon.ShowBalloonTip(400);
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.Visibility = Visibility.Visible;
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }




        /// <summary>
        /// /////////Чтение настроек
        /// </summary>
        private async void Settings()
        {
            await Task.Run(() => ReadSettingAsync(this.buttonStop, EventArgs.Empty));
            if(toRun==false)
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
        }
        private async void button_ClickStop(object sender, EventArgs e)
        {
            await Task.Run(() => button_ClickStopAsync(this.buttonStop, EventArgs.Empty));
        }
        //чтение настроек
        private async Task ReadSettingAsync(object sender, EventArgs e)
        {
            
            if (File.Exists("settings.conf"))
                using (FileStream fs = new FileStream("settings.conf", FileMode.OpenOrCreate))
                {
                    Conf readConf = await JsonSerializer.DeserializeAsync<Conf>(fs);
                    if (readConf.autostart == "true")
                    {
                        toRun = false;
                        Dispatcher.Invoke(() =>
                        {
                            CheckBox1.IsChecked = true;
                        });
                    }
                    else
                    {
                        toRun = true;
                        Dispatcher.Invoke(() =>
                        {
                            CheckBox1.IsChecked = false;
                        });
                    }
                    USER = readConf.user;
                    POOL = readConf.pool;
                    Dispatcher.Invoke(() =>
                    {
                        textBoxUser.Text = USER;
                        textBoxPool.Text = POOL;
                    });
                }
        }
        //ADD AR
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            const string applicationName = "IdlingMiner";
            const string pathRegistryKeyStartup =
                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey registryKeyStartup =
                        Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
            {
                registryKeyStartup.SetValue(
                    applicationName,
                    string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));
            }
        }
        //DEL AR
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            const string applicationName = "IdlingMiner";
            const string pathRegistryKeyStartup =
                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using (RegistryKey registryKeyStartup =
                        Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
            {
                registryKeyStartup.DeleteValue(applicationName, false);
            }
        }



        //Сохраненние настроек
        private async Task button_ClickStopAsync(object sender, EventArgs e)
        {
            //
            if (!File.Exists("settings.conf"))
            {
                using (FileStream fs = new FileStream("settings.conf", FileMode.OpenOrCreate))
                {
                    Conf newConf = new Conf { autostart = "false" , pool = POOL, user= USER};
                    Dispatcher.Invoke(() =>
                    {
                        if ((bool)CheckBox1.IsChecked)
                            newConf.autostart = "true";
                    });
                    await JsonSerializer.SerializeAsync<Conf>(fs, newConf);
                }
            }
            else
            {
                File.Delete("settings.conf");
                using (FileStream fs = new FileStream("settings.conf", FileMode.OpenOrCreate))
                {
                    Conf newConf= null;
                    Dispatcher.Invoke(() =>
                    {
                        newConf = new Conf { autostart = "false", pool = textBoxPool.Text.ToString(), user = textBoxUser.Text.ToString() };
                    
                        if ((bool)CheckBox1.IsChecked)
                            newConf.autostart = "true";
                    });
                    await JsonSerializer.SerializeAsync<Conf>(fs, newConf);
                }
            }

        }
    }
}
