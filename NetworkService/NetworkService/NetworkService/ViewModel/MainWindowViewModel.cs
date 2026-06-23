using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkService.ViewModel
{
    // NOVO: Tipovi akcija koje centralni magacin podržava za poništavanje
    public enum UndoActionType { Drop, Free, Move, Connect, AddEntity, DeleteEntity }

    // NOVO: Klasa koja drži sve potrebne podatke o izvršenoj akciji
    public class UndoAction
    {
        public UndoActionType Type { get; set; }
        public int SourceSlot { get; set; }
        public int TargetSlot { get; set; }
        public RoadEntity Entity { get; set; }
        public object Line { get; set; } // Koristi se object radi sprečavanja kružnih referenci
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object currentViewModel;
        private bool isHelpEnabled = true;
        private TcpListener server;
        private Thread listenThread;
        private string jedinstvenaPutanjaLoga;

        // Putanja za čuvanje entiteta unutar bin folder-a aplikacije
        private readonly string putanjaBazeEntiteta = AppDomain.CurrentDomain.BaseDirectory + "sačuvani_putevi.txt";

        public NetworkEntitiesViewModel NetworkEntitiesVM { get; set; }
        public NetworkDisplayViewModel NetworkDisplayVM { get; set; }
        public MeasurementGraphViewModel MeasurementGraphVM { get; set; }

        public static ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();

        // NOVO: Centralni magacin u koji se guraju sve akcije iz svih ekrana
        public Stack<UndoAction> UndoStack { get; set; } = new Stack<UndoAction>();

        public ICommand NavigationCommand { get; set; }
        public ICommand ToggleHelpCommand { get; set; }

        public object CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                OnPropertyChanged("CurrentViewModel");
            }
        }

        public bool IsHelpEnabled
        {
            get { return isHelpEnabled; }
            set
            {
                isHelpEnabled = value;
                OnPropertyChanged("IsHelpEnabled");
            }
        }

        public MainWindowViewModel()
        {
            NavigationCommand = new RelayCommand(ExecuteNavigation);
            ToggleHelpCommand = new RelayCommand(ExecuteToggleHelp);

            NetworkEntitiesVM = new NetworkEntitiesViewModel();
            NetworkDisplayVM = new NetworkDisplayViewModel();
            MeasurementGraphVM = new MeasurementGraphViewModel();

            CurrentViewModel = NetworkEntitiesVM;

            // 1. Kreiramo čist log fajl pri startu aplikacije
            napraviLog();

            // 2. Pokrećemo mrežni server na portu koji simulator traži
            StartServer();

            // 3. Učitavamo prethodno sačuvane puteve sa diska
            UcitajPodatke();
        }

        // JAVNA METODA ZA SNIMANJE TRENUTNOG STANJA U FAJL
        public void SnimiPodatke()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(putanjaBazeEntiteta, false))
                {
                    foreach (var road in Roads)
                    {
                        string tipNaziv = (road.Type != null) ? road.Type.Name : "IA";
                        writer.WriteLine($"{road.ID}|{road.Name}|{tipNaziv}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri čuvanju entiteta: {ex.Message}");
            }
        }

        // METODA ZA AUTOMATSKO UČITAVANJE PRI STARTU
        private void UcitajPodatke()
        {
            if (!File.Exists(putanjaBazeEntiteta)) return;

            try
            {
                string[] linije = File.ReadAllLines(putanjaBazeEntiteta);
                Roads.Clear();

                foreach (string linija in linije)
                {
                    if (string.IsNullOrWhiteSpace(linija)) continue;

                    string[] delovi = linija.Split('|');
                    if (delovi.Length == 3)
                    {
                        int id = int.Parse(delovi[0]);
                        string name = delovi[1];
                        string tipNaziv = delovi[2];

                        string putanjaIkonice = tipNaziv == "IA" ? "Slike/road_ia.png" : "Slike/road_ib.png";
                        RoadType tip = new RoadType(tipNaziv, putanjaIkonice);

                        Roads.Add(new RoadEntity { ID = id, Name = name, Type = tip, Value = 0 });
                    }
                }

                if (NetworkEntitiesVM != null)
                {
                    NetworkEntitiesVM.RefreshTable();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju entiteta: {ex.Message}");
            }
        }

        private void napraviLog()
        {
            try
            {
                string vremenskiSufiks = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                // VRACENO: Tvoja originalna apsolutna putanja
                jedinstvenaPutanjaLoga = $@"C:\Users\strah\source\repos\HCI_PZ2\NetworkService\NetworkService\NetworkService\bin\Log_{vremenskiSufiks}.txt";

                using (FileStream fs = File.Create(jedinstvenaPutanjaLoga))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine($"=== TRAFFIC LOG START: {DateTime.Now:dd.MM.yyyy. HH:mm:ss} ===");
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri kreiranju jedinstvenog loga u bin folderu: {ex.Message}", "Sistemska Greška");
            }
        }

        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), 25675);
                server.Start();

                listenThread = new Thread(new ThreadStart(ListenForClients));
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri podizanju servera: {ex.Message}");
            }
        }

        private void ListenForClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    byte[] message = new byte[1024];
                    int bytesRead = stream.Read(message, 0, message.Length);
                    string incomingData = Encoding.ASCII.GetString(message, 0, bytesRead);

                    if (incomingData == "Need object count")
                    {
                        byte[] responseData = Encoding.ASCII.GetBytes(Roads.Count.ToString());
                        stream.Write(responseData, 0, responseData.Length);
                    }
                    else if (incomingData.StartsWith("Entitet_"))
                    {
                        string[] parts = incomingData.Split(':');
                        int entityIndex = int.Parse(parts[0].Split('_')[1]);
                        double newValue = double.Parse(parts[1]);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (entityIndex >= 0 && entityIndex < Roads.Count)
                            {
                                var road = Roads[entityIndex];
                                road.Value = newValue;

                                UpisiULogFajl(road);

                                // 1. Osvežavanje mrežnog Canvas prikaza
                                if (NetworkDisplayVM != null && NetworkDisplayVM.CanvasSlots != null)
                                {
                                    foreach (var slot in NetworkDisplayVM.CanvasSlots)
                                    {
                                        if (slot != null && slot.Road != null && slot.Road.ID == road.ID)
                                        {
                                            var privremeniPut = slot.Road;
                                            slot.Road = null;
                                            slot.Road = privremeniPut;
                                        }
                                    }
                                }

                                // 2. Instant osvežavanje grafikona ako je ovaj put trenutno otvoren na njemu
                                if (MeasurementGraphVM != null && MeasurementGraphVM.SelectedRoad != null)
                                {
                                    if (MeasurementGraphVM.SelectedRoad.ID == road.ID)
                                    {
                                        MeasurementGraphVM.RefreshGraph();
                                    }
                                }
                            }
                        });
                    }

                    stream.Close();
                    client.Close();
                }
                catch (Exception)
                {
                    // Ignoriši mrežne prekide simulatora
                }
            }
        }

        private void UpisiULogFajl(RoadEntity road)
        {
            if (string.IsNullOrEmpty(jedinstvenaPutanjaLoga)) return;

            try
            {
                using (StreamWriter writer = File.AppendText(jedinstvenaPutanjaLoga))
                {
                    string typeName = (road.Type != null) ? road.Type.Name : "Unknown";
                    string roadName = !string.IsNullOrEmpty(road.Name) ? road.Name : "Unnamed";

                    string logLine = $"[{DateTime.Now:dd.MM.yyyy. HH:mm:ss}] ID: {road.ID} | Name: {roadName} | Type: {typeName} | Value: {road.Value:F1}";

                    writer.WriteLine(logLine);
                    writer.Flush();
                }
            }
            catch (Exception) { }
        }

        private void ExecuteNavigation(object parameter)
        {
            string destination = parameter as string;
            switch (destination)
            {
                case "entities":
                    CurrentViewModel = NetworkEntitiesVM;
                    break;

                case "display":
                    NetworkDisplayVM.RefreshTreeView();
                    CurrentViewModel = NetworkDisplayVM;
                    break;

                case "graph":
                    CurrentViewModel = MeasurementGraphVM;
                    break;
            }
        }

        private void ExecuteToggleHelp(object parameter)
        {
            IsHelpEnabled = !IsHelpEnabled;

            var temp = CurrentViewModel;
            CurrentViewModel = null;
            CurrentViewModel = temp;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}