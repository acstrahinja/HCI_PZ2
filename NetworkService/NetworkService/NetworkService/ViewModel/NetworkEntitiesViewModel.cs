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

        // Svojstva za RadioButton pretragu
        private bool searchByType = true;
        private bool searchByName = false;
        private string searchText;

        public ObservableCollection<RoadType> RoadTypes { get; set; }
        public ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();

        public ICommand AddRoadCommand { get; set; }
        public ICommand DeleteRoadCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ResetSearchCommand { get; set; }

        public string NewRoadId { get { return newRoadId; } set { newRoadId = value; OnPropertyChanged("NewRoadId"); } }
        public string NewRoadName { get { return newRoadName; } set { newRoadName = value; OnPropertyChanged("NewRoadName"); } }
        public RoadType SelectedRoadType { get { return selectedRoadType; } set { selectedRoadType = value; OnPropertyChanged("SelectedRoadType"); } }
        public RoadEntity SelectedRoad { get { return selectedRoad; } set { selectedRoad = value; OnPropertyChanged("SelectedRoad"); } }

        public bool SearchByType
        {
            get { return searchByType; }
            set { searchByType = value; if (value) SearchByName = false; OnPropertyChanged("SearchByType"); }
        }

        public bool SearchByName
        {
            get { return searchByName; }
            set { searchByName = value; if (value) SearchByType = false; OnPropertyChanged("SearchByName"); }
        }

        public string SearchText { get { return searchText; } set { searchText = value; OnPropertyChanged("SearchText"); } }

        public NetworkEntitiesViewModel()
        {
            RoadTypes = new ObservableCollection<RoadType>
            {
                new RoadType("IA", "Slike/road_ia.png"),
                new RoadType("IB", "Slike/road_ib.png")
            };

            AddRoadCommand = new RelayCommand(ExecuteAddRoad);
            DeleteRoadCommand = new RelayCommand(ExecuteDeleteRoad);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ResetSearchCommand = new RelayCommand(ExecuteResetSearch);

            RefreshTable();
        }

        private void RefreshTable()
        {
            Roads.Clear();
            foreach (var r in MainWindowViewModel.Roads) Roads.Add(r);
        }

        private void ExecuteSearch(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                RefreshTable();
                return;
            }

            string term = SearchText.Trim().ToLower();
            Roads.Clear();

            foreach (var r in MainWindowViewModel.Roads)
            {
                if (SearchByType && r.Type.Name.ToLower() == term)
                {
                    Roads.Add(r);
                }
                else if (SearchByName && r.Name.ToLower().Contains(term))
                {
                    Roads.Add(r);
                }
            }
        }

        private void ExecuteResetSearch(object parameter)
        {
            SearchText = string.Empty;
            RefreshTable();
        }

        private void ExecuteAddRoad(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewRoadId) || string.IsNullOrWhiteSpace(NewRoadName) || SelectedRoadType == null)
            {
                MessageBox.Show("All fields must be filled correctly!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(NewRoadId, out int id))
            {
                MessageBox.Show("ID must be an integer number!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var r in MainWindowViewModel.Roads)
            {
                if (r.ID == id)
                {
                    MessageBox.Show("This ID already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            MainWindowViewModel.Roads.Add(new RoadEntity { ID = id, Name = NewRoadName, Type = SelectedRoadType, Value = 0 });
            RefreshTable();

            NewRoadId = string.Empty; NewRoadName = string.Empty; SelectedRoadType = null;
        }

        private void ExecuteDeleteRoad(object parameter)
        {
            var toRemove = new System.Collections.Generic.List<RoadEntity>();
            foreach (var r in MainWindowViewModel.Roads) if (r.IsSelected) toRemove.Add(r);

            if (toRemove.Count == 0)
            {
                MessageBox.Show("Please check the entities you want to delete!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var r in toRemove) MainWindowViewModel.Roads.Remove(r);
            RefreshTable();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}