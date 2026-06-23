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

    public class GraphLine : INotifyPropertyChanged
    {
        private int fromSlot;
        private int toSlot;
        private double x1, y1, x2, y2;

        public int FromSlot { get => fromSlot; set { fromSlot = value; RecalculateCoordinates(); } }
        public int ToSlot { get => toSlot; set { toSlot = value; RecalculateCoordinates(); } }

        public double X1 { get => x1; set { x1 = value; OnPropertyChanged("X1"); } }
        public double Y1 { get => y1; set { y1 = value; OnPropertyChanged("Y1"); } }
        public double X2 { get => x2; set { x2 = value; OnPropertyChanged("X2"); } }
        public double Y2 { get => y2; set { y2 = value; OnPropertyChanged("Y2"); } }

        public GraphLine(int from, int to)
        {
            fromSlot = from;
            toSlot = to;
            RecalculateCoordinates();
        }

        public void RecalculateCoordinates()
        {
            double slotWidth = 510.0 / 3.0;
            double slotHeight = 450.0 / 4.0;

            int fromRow = fromSlot / 3;
            int fromCol = fromSlot % 3;
            X1 = (fromCol * slotWidth) + (slotWidth / 2.0);
            Y1 = (fromRow * slotHeight) + (slotHeight / 2.0);

            int toRow = toSlot / 3;
            int toCol = toSlot % 3;
            X2 = (toCol * slotWidth) + (slotWidth / 2.0);
            Y2 = (toRow * slotHeight) + (slotHeight / 2.0);
        }

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

        public ObservableCollection<Slot> CanvasSlots { get; set; }
        public ObservableCollection<GraphLine> Lines { get; set; } = new ObservableCollection<GraphLine>();

        public ICommand DropCommand { get; set; }
        public ICommand FreeSlotCommand { get; set; }
        public ICommand ConnectSlotsCommand { get; set; }
        public ICommand UndoCommand { get; set; }

        private int firstSelectedSlot = -1;
        public int FirstSelectedSlot
        {
            get => firstSelectedSlot;
            set { firstSelectedSlot = value; OnPropertyChanged("FirstSelectedSlot"); }
        }

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
            ConnectSlotsCommand = new RelayCommand(ExecuteConnectSlots);
            UndoCommand = new RelayCommand(ExecuteUndo);
        }

        public void RefreshTreeView()
        {
            var groups = new Dictionary<string, RoadTypeGroup>();
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
            if (parameter == null) return;
            int index = Convert.ToInt32(parameter);

            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM == null) return;

            if (SelectedEntity == null)
            {
                if (CanvasSlots[index].IsOccupied)
                {
                    SelectedEntity = CanvasSlots[index].Road;
                    FirstSelectedSlot = index;
                }
                return;
            }

            if (!CanvasSlots[index].IsOccupied)
            {
                if (FirstSelectedSlot != -1)
                {
                    mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.Move, SourceSlot = FirstSelectedSlot, TargetSlot = index, Entity = SelectedEntity });

                    CanvasSlots[FirstSelectedSlot] = new Slot();
                    foreach (var line in Lines)
                    {
                        if (line.FromSlot == FirstSelectedSlot) line.FromSlot = index;
                        if (line.ToSlot == FirstSelectedSlot) line.ToSlot = index;
                    }
                    FirstSelectedSlot = -1;
                }
                else
                {
                    mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.Drop, TargetSlot = index, Entity = SelectedEntity });
                }

                CanvasSlots[index] = new Slot { Road = SelectedEntity };
                RefreshTreeView();
                SelectedEntity = null;
            }
        }

        private void ExecuteFreeSlot(object parameter)
        {
            if (parameter == null) return;
            int index = Convert.ToInt32(parameter);

            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM == null) return;

            if (CanvasSlots[index].IsOccupied)
            {
                mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.Free, SourceSlot = index, Entity = CanvasSlots[index].Road });

                CanvasSlots[index] = new Slot();

                for (int i = Lines.Count - 1; i >= 0; i--)
                {
                    if (Lines[i].FromSlot == index || Lines[i].ToSlot == index)
                    {
                        Lines.RemoveAt(i);
                    }
                }

                RefreshTreeView();
            }
        }

        private void ExecuteConnectSlots(object parameter)
        {
            if (parameter == null) return;
            int index = Convert.ToInt32(parameter);

            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM == null) return;

            if (!CanvasSlots[index].IsOccupied) return;

            if (FirstSelectedSlot == -1)
            {
                FirstSelectedSlot = index;
            }
            else
            {
                int first = FirstSelectedSlot;
                int second = index;
                FirstSelectedSlot = -1;

                if (first == second) return;

                foreach (var line in Lines)
                {
                    if ((line.FromSlot == first && line.ToSlot == second) ||
                        (line.FromSlot == second && line.ToSlot == first))
                    {
                        return;
                    }
                }

                GraphLine newLine = new GraphLine(first, second);
                Lines.Add(newLine);

                mainWindowVM.UndoStack.Push(new UndoAction { Type = UndoActionType.Connect, Line = newLine });
            }
        }

        private void ExecuteUndo(object parameter)
        {
            var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainWindowVM == null || mainWindowVM.UndoStack.Count == 0)
            {
                MessageBox.Show("Nema komandi za poništavanje!", "Undo Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            UndoAction lastAction = mainWindowVM.UndoStack.Pop();

            switch (lastAction.Type)
            {
                case UndoActionType.Drop:
                    CanvasSlots[lastAction.TargetSlot] = new Slot();
                    break;

                case UndoActionType.Free:
                    CanvasSlots[lastAction.SourceSlot] = new Slot { Road = lastAction.Entity };
                    break;

                case UndoActionType.Move:
                    CanvasSlots[lastAction.TargetSlot] = new Slot();
                    CanvasSlots[lastAction.SourceSlot] = new Slot { Road = lastAction.Entity };

                    foreach (var line in Lines)
                    {
                        if (line.FromSlot == lastAction.TargetSlot) line.FromSlot = lastAction.SourceSlot;
                        if (line.ToSlot == lastAction.TargetSlot) line.ToSlot = lastAction.SourceSlot;
                    }
                    break;

                case UndoActionType.Connect:
                    // KASTOVANJE I ISPRAVLJANJE GREŠKE: Bezbedno pretvaramo object u GraphLine objekat
                    var linijaZaBrisanje = lastAction.Line as GraphLine;
                    if (linijaZaBrisanje != null)
                    {
                        GraphLine pronadjenaLinija = null;
                        foreach (var l in Lines)
                        {
                            if (l.FromSlot == linijaZaBrisanje.FromSlot && l.ToSlot == linijaZaBrisanje.ToSlot)
                            {
                                pronadjenaLinija = l;
                                break;
                            }
                        }

                        if (pronadjenaLinija != null)
                        {
                            Lines.Remove(pronadjenaLinija);
                        }
                    }
                    break;

                case UndoActionType.AddEntity:
                    if (MainWindowViewModel.Roads.Contains(lastAction.Entity))
                    {
                        MainWindowViewModel.Roads.Remove(lastAction.Entity);
                        ObrisiSaMrezePoId(lastAction.Entity.ID);
                    }
                    break;

                case UndoActionType.DeleteEntity:
                    if (!MainWindowViewModel.Roads.Contains(lastAction.Entity))
                    {
                        MainWindowViewModel.Roads.Add(lastAction.Entity);
                    }
                    break;
            }

            RefreshTreeView();

            if (mainWindowVM.NetworkEntitiesVM != null)
            {
                mainWindowVM.NetworkEntitiesVM.RefreshTable();
            }
            mainWindowVM.SnimiPodatke();
        }

        public void ObrisiSaMrezePoId(int roadId)
        {
            for (int i = 0; i < 12; i++)
            {
                if (CanvasSlots[i].Road != null && CanvasSlots[i].Road.ID == roadId)
                {
                    CanvasSlots[i] = new Slot();
                    for (int j = Lines.Count - 1; j >= 0; j--)
                    {
                        if (Lines[j].FromSlot == i || Lines[j].ToSlot == i) Lines.RemoveAt(j);
                    }
                }
            }
            RefreshTreeView();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}