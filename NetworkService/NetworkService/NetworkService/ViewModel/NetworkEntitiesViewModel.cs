using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : INotifyPropertyChanged
    {
        private string newRoadId;
        private string newRoadName;
        private RoadType selectedRoadType;
        private RoadEntity selectedRoad;

        public ObservableCollection<RoadType> RoadTypes { get; set; }
        public ObservableCollection<RoadEntity> Roads
        {
            get { return MainWindowViewModel.Roads; }
        }

        public ICommand AddRoadCommand { get; set; }
        public ICommand DeleteRoadCommand { get; set; }

        public string NewRoadId
        {
            get { return newRoadId; }
            set { newRoadId = value; OnPropertyChanged("NewRoadId"); }
        }

        public string NewRoadName
        {
            get { return newRoadName; }
            set { newRoadName = value; OnPropertyChanged("NewRoadName"); }
        }

        public RoadType SelectedRoadType
        {
            get { return selectedRoadType; }
            set { selectedRoadType = value; OnPropertyChanged("SelectedRoadType"); }
        }

        public RoadEntity SelectedRoad
        {
            get { return selectedRoad; }
            set { selectedRoad = value; OnPropertyChanged("SelectedRoad"); }
        }

        public NetworkEntitiesViewModel()
        {
            RoadTypes = new ObservableCollection<RoadType>
            {
                new RoadType("IA", "Slike/road_ia.png"),
                new RoadType("IB", "Slike/road_ib.png")
            };

            AddRoadCommand = new RelayCommand(ExecuteAddRoad);
            DeleteRoadCommand = new RelayCommand(ExecuteDeleteRoad);
        }

        private void ExecuteAddRoad(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewRoadId) || string.IsNullOrWhiteSpace(NewRoadName) || SelectedRoadType == null)
            {
                MessageBox.Show("Sva polja moraju biti ispravno popunjena!", "Greška pri unosu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(NewRoadId, out int id))
            {
                MessageBox.Show("ID mora biti ceo broj!", "Greška pri unosu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var r in Roads)
            {
                if (r.ID == id)
                {
                    MessageBox.Show("Objekat sa ovim ID-jem već postoji!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            RoadEntity newEntity = new RoadEntity
            {
                ID = id,
                Name = NewRoadName,
                Type = SelectedRoadType,
                Value = 0
            };

            Roads.Add(newEntity);

            NewRoadId = string.Empty;
            NewRoadName = string.Empty;
            SelectedRoadType = null;
        }

        private void ExecuteDeleteRoad(object parameter)
        {
            if (SelectedRoad == null)
            {
                MessageBox.Show("Morate selektovati entitet iz tabele za brisanje!", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Roads.Remove(SelectedRoad);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}