using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object currentViewModel;
        private bool isHelpEnabled = true;

        // Globalna statička lista za entitete
        public static ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();

        // ISPRAVLJENO: Tačan naziv komande koji traže i XAML i .xaml.cs
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
            // Povezivanje komandi sa metodama
            NavigationCommand = new RelayCommand(ExecuteNavigation);
            ToggleHelpCommand = new RelayCommand(ExecuteToggleHelp);

            // Početni prikaz
            CurrentViewModel = new NetworkEntitiesViewModel();
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
                    MessageBox.Show("Navigation to: Network Display View");
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