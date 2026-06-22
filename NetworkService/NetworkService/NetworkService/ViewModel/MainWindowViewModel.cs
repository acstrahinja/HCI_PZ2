using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading; // Potrebno za DispatcherTimer
using System.IO;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object currentViewModel;
        private bool isHelpEnabled = true;
        private DispatcherTimer simulationTimer;
        private Random random = new Random();

        // Polje koje čuva dinamičku putanju do jedinstvenog fajla za trenutnu sesiju
        private string jedinstvenaPutanjaLoga;

        // 1. Instance ViewModela kreiramo jednom da bi pamtile stanje pri menjanju tabova
        public NetworkEntitiesViewModel NetworkEntitiesVM { get; set; }
        public NetworkDisplayViewModel NetworkDisplayVM { get; set; }
        public MeasurementGraphViewModel MeasurementGraphVM { get; set; }

        // Globalna statička lista u kojoj se čuvaju svi putevi (entiteti) u aplikaciji
        public static ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();

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
            // Inicijalizacija komandi
            NavigationCommand = new RelayCommand(ExecuteNavigation);
            ToggleHelpCommand = new RelayCommand(ExecuteToggleHelp);

            // 2. Kreiramo sve ViewModele odmah na početku
            NetworkEntitiesVM = new NetworkEntitiesViewModel();
            NetworkDisplayVM = new NetworkDisplayViewModel();
            MeasurementGraphVM = new MeasurementGraphViewModel();

            // Početni ekran je Network Entities
            CurrentViewModel = NetworkEntitiesVM;

            // KREIRANJE JEDINSTVENOG FAJLA NA STARTU
            napraviLog();

            // POKRETANJE SIMULATORA MERENJA
            StartSimulation();
        }

        // FUNKCIJA KOJA GENERIŠE JEDINSTVENO IME I KREIRA FAJL U BIN FOLDERU
        private void napraviLog()
        {
            try
            {
                // Pravimo vremenski sufiks (npr. Log_22_06_2026_21_15_30.txt)
                string vremenskiSufiks = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

                // Spajamo tvoju putanju do bin foldera sa unikatnim imenom fajla
                jedinstvenaPutanjaLoga = $@"C:\Users\strah\source\repos\HCI_PZ2\NetworkService\NetworkService\NetworkService\bin\Log_{vremenskiSufiks}.txt";

                // Eksplicitno kreiranje fajla na disku
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

        private void StartSimulation()
        {
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromSeconds(3); // Osvežavanje na svake 3 sekunde
            simulationTimer.Tick += SimulationTimer_Tick;
            simulationTimer.Start();
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (Roads.Count == 0) return;
            if (string.IsNullOrEmpty(jedinstvenaPutanjaLoga)) return;

            try
            {
                // Otvaramo tačno onaj unikatni fajl koji je napravljen u metodi napraviLog()
                using (StreamWriter writer = File.AppendText(jedinstvenaPutanjaLoga))
                {
                    foreach (var road in Roads)
                    {
                        road.Value = random.Next(0, 151);

                        string typeName = (road.Type != null) ? road.Type.Name : "Unknown";
                        string roadName = !string.IsNullOrEmpty(road.Name) ? road.Name : "Unnamed";

                        string logLine = $"[{DateTime.Now:dd.MM.yyyy. HH:mm:ss}] ID: {road.ID} | Name: {roadName} | Type: {typeName} | Value: {road.Value:F1}";

                        writer.WriteLine(logLine);
                    }
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