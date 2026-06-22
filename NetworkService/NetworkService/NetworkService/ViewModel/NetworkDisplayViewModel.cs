using NetworkService.Model;
using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class RoadTypeGroup
    {
        public string Name { get; set; }
        public ObservableCollection<RoadEntity> Roads { get; set; } = new ObservableCollection<RoadEntity>();
    }

    // NOVO: Klasa koja predstavlja jedno od 12 polja na mapi
    public class Slot : INotifyPropertyChanged
    {
        private RoadEntity road;
        public RoadEntity Road
        {
            get { return road; }
            set
            {
                road = value;
                OnPropertyChanged("Road");
                OnPropertyChanged("IsOccupied");
                OnPropertyChanged("IsFree");
            }
        }

        public bool IsOccupied => Road != null;
        public Visibility IsOccupiedVisibility => Road != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsFreeVisibility => Road == null ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NetworkDisplayViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RoadTypeGroup> treeViewItems;
        public ObservableCollection<RoadTypeGroup> TreeViewItems
        {
            get { return treeViewItems; }
            set { treeViewItems = value; OnPropertyChanged("TreeViewItems"); }
        }

        // 12 slotova na mapi tipa Slot umesto RoadEntity
        public ObservableCollection<Slot> CanvasSlots { get; set; }

        public ICommand DropCommand { get; set; }
        public ICommand FreeSlotCommand { get; set; }

        private RoadEntity selectedEntity;
        public RoadEntity SelectedEntity
        {
            get { return selectedEntity; }
            set { selectedEntity = value; OnPropertyChanged("SelectedEntity"); }
        }

        public NetworkDisplayViewModel()
        {
            CanvasSlots = new ObservableCollection<Slot>();
            for (int i = 0; i < 12; i++)
            {
                CanvasSlots.Add(new Slot());
            }

            RefreshTreeView();

            DropCommand = new RelayCommand(ExecuteDrop);
            FreeSlotCommand = new RelayCommand(ExecuteFreeSlot);
        }

        public void RefreshTreeView()
        {
            var groups = new Dictionary<string, RoadTypeGroup>();

            // Pravimo listu zauzetih entiteta radi provere
            var occupiedRoads = new List<RoadEntity>();
            foreach (var slot in CanvasSlots)
            {
                if (slot.Road != null) occupiedRoads.Add(slot.Road);
            }

            foreach (var road in MainWindowViewModel.Roads)
            {
                if (occupiedRoads.Contains(road)) continue;

                string typeName = road.Type != null ? road.Type.Name : "Unknown";

                if (!groups.ContainsKey(typeName))
                {
                    groups[typeName] = new RoadTypeGroup { Name = typeName };
                }
                groups[typeName].Roads.Add(road);
            }

            TreeViewItems = new ObservableCollection<RoadTypeGroup>(groups.Values);
        }

        private void ExecuteDrop(object parameter)
        {
            if (SelectedEntity == null || parameter == null) return;

            int index = Convert.ToInt32(parameter);

            if (!CanvasSlots[index].IsOccupied)
            {
                // PRAVO REŠENJE: Pravimo novi objekat slota i menjamo ga na indeksu
                // To primorava ItemsControl da kompletno ponovo iscrta ovaj prozorčić
                var newSlot = new Slot { Road = SelectedEntity };
                CanvasSlots[index] = newSlot;

                RefreshTreeView();
                SelectedEntity = null;
            }
        }

        private void ExecuteFreeSlot(object parameter)
        {
            if (parameter == null) return;

            int index = Convert.ToInt32(parameter);
            if (CanvasSlots[index].IsOccupied)
            {
                // Vraćamo na potpuno prazan Slot objekat
                CanvasSlots[index] = new Slot();
                RefreshTreeView();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}