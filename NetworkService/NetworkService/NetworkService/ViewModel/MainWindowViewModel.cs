using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace PSI_IUIS___PZ2___početni_projekat
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static ObservableCollection<object> Roads { get; set; } = new ObservableCollection<object>();
        private readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.txt");

        private object currentViewModel;
        private bool isHelpEnabled = true;

        public object CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                if (currentViewModel != value)
                {
                    currentViewModel = value;
                    OnPropertyChanged("CurrentViewModel");
                }
            }
        }

        public bool IsHelpEnabled
        {
            get { return isHelpEnabled; }
            set
            {
                if (isHelpEnabled != value)
                {
                    isHelpEnabled = value;
                    OnPropertyChanged("IsHelpEnabled");
                }
            }
        }

        // DODATE KOMANDE KOJE SU FALILE ZA .CS FAJL
        public ICommand NavigateCommand { get; set; }
        public ICommand ToggleHelpCommand { get; set; }

        public MainWindowViewModel()
        {
            // Inicijalizacija komandi
            NavigateCommand = new RelayCommand(ExecuteNavigation);
            ToggleHelpCommand = new RelayCommand(ExecuteToggleHelp);

            createListener();
        }

        private void ExecuteNavigation(object parameter)
        {
            string destination = parameter as string;
            switch (destination)
            {
                case "entities":
                    MessageBox.Show("Navigacija na: Network Entities View");
                    break;
                case "display":
                    MessageBox.Show("Navigacija na: Network Display View");
                    break;
                case "graph":
                    MessageBox.Show("Navigacija na: Measurement Graph View");
                    break;
            }
        }

        private void ExecuteToggleHelp(object parameter)
        {
            IsHelpEnabled = !IsHelpEnabled;
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var tcpClient = tcp.AcceptTcpClient();
                        ThreadPool.QueueUserWorkItem(param =>
                        {
                            try
                            {
                                NetworkStream stream = tcpClient.GetStream();
                                string incoming;
                                byte[] bytes = new byte[1024];
                                int i = stream.Read(bytes, 0, bytes.Length);

                                incoming = Encoding.ASCII.GetString(bytes, 0, i);

                                if (incoming.Equals("Need object count"))
                                {
                                    int currentCount = Roads.Count;
                                    byte[] data = Encoding.ASCII.GetBytes(currentCount.ToString());
                                    stream.Write(data, 0, data.Length);
                                }
                                else
                                {
                                    Console.WriteLine(incoming);
                                }
                            }
                            catch (Exception ex) { Console.WriteLine(ex.Message); }
                            finally { tcpClient.Close(); }
                        }, null);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); break; }
                }
            });
            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}