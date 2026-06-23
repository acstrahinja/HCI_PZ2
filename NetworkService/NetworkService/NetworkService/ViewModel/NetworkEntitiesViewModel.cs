using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;



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

        // Svojstva za kontrolu vidljivosti custom dijaloga za brisanje
        private Visibility isConfirmDeleteVisible = Visibility.Collapsed;
        private System.Collections.Generic.List<RoadEntity> temporaryToRemove;

        // Svojstva za upravljanje prilagođenom Toast notifikacijom (Bez MessageBox-a)
        private Visibility isToastVisible = Visibility.Collapsed;
        private string toastTitle;
        private string toastContent;
        private string toastType;
        private System.Windows.Media.Brush toastBackground;
        private DispatcherTimer toastTimer;

        public ObservableCollection<RoadType> RoadTypes { get; set; }
        public ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();

        public ICommand AddRoadCommand { get; set; }
        public ICommand DeleteRoadCommand { get; set; }
        public ICommand ConfirmDeleteCommand { get; set; }
        public ICommand CancelDeleteCommand { get; set; }
        public ICommand CloseToastCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ResetSearchCommand { get; set; }
        public ICommand UndoCommand { get; set; }

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

        public Visibility IsConfirmDeleteVisible
        {
            get { return isConfirmDeleteVisible; }
            set { isConfirmDeleteVisible = value; OnPropertyChanged("IsConfirmDeleteVisible"); }
        }

        // Svojstva za data-binding Toast panela u XAML-u
        public Visibility IsToastVisible { get => isToastVisible; set { isToastVisible = value; OnPropertyChanged("IsToastVisible"); } }
        public string ToastTitle { get => toastTitle; set { toastTitle = value; OnPropertyChanged("ToastTitle"); } }
        public string ToastContent { get => toastContent; set { toastContent = value; OnPropertyChanged("ToastContent"); } }
        public string ToastType { get => toastType; set { toastType = value; OnPropertyChanged("ToastType"); } }
        public System.Windows.Media.Brush ToastBackground { get => toastBackground; set { toastBackground = value; OnPropertyChanged("ToastBackground"); } }

        public NetworkEntitiesViewModel()
        {
            // ISPRAVLJENO: Korišćenje nepogrešivog Pack URI formata resursa umesto apsolutnih putanja sa diska
            RoadTypes = new ObservableCollection<RoadType>
            {
                new RoadType("IA", "pack://application:,,,/Slike/road_ia.png"),
                new RoadType("IB", "pack://application:,,,/Slike/road_ib.png")
            };

            AddRoadCommand = new RelayCommand(ExecuteAddRoad);
            DeleteRoadCommand = new RelayCommand(ExecuteDeleteRoad);
            ConfirmDeleteCommand = new RelayCommand(ExecuteConfirmDelete);
            CancelDeleteCommand = new RelayCommand(ExecuteCancelDelete);
            CloseToastCommand = new RelayCommand(p => IsToastVisible = Visibility.Collapsed);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ResetSearchCommand = new RelayCommand(ExecuteResetSearch);
            UndoCommand = new RelayCommand(ExecuteUndoFromTable);

            // TAČNO 3 SEKUNDE: Inicijalizacija tajmera za automatsko zatvaranje
            toastTimer = new DispatcherTimer();
            toastTimer.Interval = TimeSpan.FromSeconds(3);
            toastTimer.Tick += (s, e) => { IsToastVisible = Visibility.Collapsed; toastTimer.Stop(); };

            RefreshTable();
        }

        private void ShowToast(string title, string content, string type)
        {
            ToastTitle = title;
            ToastContent = content;
            ToastType = $"Notification Type: {type}";

            if (type == "Success")
                ToastBackground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2ECC71"));
            else if (type == "Warning")
                ToastBackground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F1C40F"));
            else
                ToastBackground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E74C3C"));

            IsToastVisible = Visibility.Visible;
            toastTimer.Stop();
            toastTimer.Start();
        }

        public void RefreshTable()
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
                ShowToast("Validation Failed", "All fields must be filled correctly before adding a record!", "Warning");
                return;
            }

            if (!int.TryParse(NewRoadId, out int id))
            {
                ShowToast("Validation Error", "The ID parameter must be an integer value!", "Error");
                return;
            }

            foreach (var r in MainWindowViewModel.Roads)
            {
                if (r.ID == id)
                {
                    ShowToast("Duplicate Entity ID", $"An object with ID {id} already exists in the system database!", "Error");
                    return;
                }
            }

            var newRoad = new RoadEntity { ID = id, Name = NewRoadName, Type = SelectedRoadType, Value = 0 };
            MainWindowViewModel.Roads.Add(newRoad);
            RefreshTable();

            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM != null)
            {
                mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.AddEntity, Entity = newRoad });
                mainWindowVM.SnimiPodatke();
            }

            ShowToast("Entity Added Successfully", $"Completed adding: ID={id}, Name={NewRoadName}, Type={SelectedRoadType.Name}", "Success");

            NewRoadId = string.Empty; NewRoadName = string.Empty; SelectedRoadType = null;
        }

        private void ExecuteDeleteRoad(object parameter)
        {
            temporaryToRemove = new System.Collections.Generic.List<RoadEntity>();
            foreach (var r in MainWindowViewModel.Roads) if (r.IsSelected) temporaryToRemove.Add(r);

            if (temporaryToRemove.Count == 0)
            {
                ShowToast("Selection Missing", "Please select at least one row checkbox inside the table to delete!", "Warning");
                return;
            }

            IsConfirmDeleteVisible = Visibility.Visible;
        }

        private void ExecuteConfirmDelete(object parameter)
        {
            if (temporaryToRemove == null || temporaryToRemove.Count == 0) return;

            int deletedCount = temporaryToRemove.Count;
            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            if (mainWindowVM != null)
            {
                foreach (var road in temporaryToRemove)
                {
                    mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.DeleteEntity, Entity = road });

                    if (mainWindowVM.NetworkDisplayVM != null)
                    {
                        mainWindowVM.NetworkDisplayVM.ObrisiSaMrezePoId(road.ID);
                    }

                    if (mainWindowVM.MeasurementGraphVM.SelectedRoad != null &&
                        mainWindowVM.MeasurementGraphVM.SelectedRoad.ID == road.ID)
                    {
                        mainWindowVM.MeasurementGraphVM.SelectedRoad = null;
                    }
                }

                mainWindowVM.NetworkDisplayVM.RefreshTreeView();
            }

            foreach (var r in temporaryToRemove) MainWindowViewModel.Roads.Remove(r);
            RefreshTable();

            if (mainWindowVM != null)
            {
                mainWindowVM.SnimiPodatke();
            }

            ShowToast("Entities Deleted Successfully", $"Completed removal of {deletedCount} selected object(s) from database.", "Success");

            IsConfirmDeleteVisible = Visibility.Collapsed;
            temporaryToRemove = null;
        }

        private void ExecuteCancelDelete(object parameter)
        {
            IsConfirmDeleteVisible = Visibility.Collapsed;
            temporaryToRemove = null;
        }

        private void ExecuteUndoFromTable(object parameter)
        {
            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM != null && mainWindowVM.NetworkDisplayVM != null)
            {
                mainWindowVM.NetworkDisplayVM.UndoCommand.Execute(parameter);
                ShowToast("Operation Reverted", "The last completed action has been successfully undone.", "Warning");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}