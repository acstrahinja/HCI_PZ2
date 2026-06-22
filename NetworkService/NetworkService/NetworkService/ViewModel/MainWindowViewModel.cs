using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading; // Potrebno za DispatcherTimer

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object currentViewModel;
        private bool isHelpEnabled = true;
        private DispatcherTimer simulationTimer;
        private Random random = new Random();

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

            // Početni ekran je Network Entities
            CurrentViewModel = new NetworkEntitiesViewModel();

            // POKRETANJE SIMULATORA MERENJA
            StartSimulation();
        }

        private void StartSimulation()
        {
            // Koristimo DispatcherTimer jer on radi na UI niti, pa WPF tabela može direktno da vidi izmene
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromSeconds(3); // Osvežavanje na svake 3 sekunde
            simulationTimer.Tick += SimulationTimer_Tick;
            simulationTimer.Start();
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (Roads.Count == 0) return;

            foreach (var road in Roads)
            {
                // OPSEG OD 0 DO 150: Brojevi preko 100 će garantovano paliti alarm s vremena na vreme
                road.Value = random.Next(0, 151);
            }
        }

        private void ExecuteNavigation(object parameter)
        {
            string destination = parameter as string;
            switch (destination)
            {
                case "entities":
                    CurrentViewModel = new NetworkEntitiesViewModel();
                    break;

                case "display":
                    CurrentViewModel = new NetworkDisplayViewModel();
                    break;

                case "graph":
                    MessageBox.Show("Navigation to: Measurement Graph View");
                    break;
            }
        }

        private void ExecuteToggleHelp(object parameter)
        {
            IsHelpEnabled = !IsHelpEnabled;

            // Forsiramo osvežavanje trenutnog ekrana kako bi stilovi sakrili ili prikazali pomoć
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