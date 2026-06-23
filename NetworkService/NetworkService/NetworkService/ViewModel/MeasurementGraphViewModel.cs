using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using NetworkService.Model;

namespace NetworkService.ViewModel
{
    public class GraphBar : INotifyPropertyChanged
    {
        private double height; // Kod G3 zahteva ovo koristimo kao PREČNIK (Size) kruga
        private double bottomPosition; // Pomoćno svojstvo za pozicioniranje od dna Canvasa
        private double leftPosition; // Pomoćno svojstvo za centriranje unutar slota od 80px
        private Brush barColor;
        private double value;
        private string labelX;

        public double Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
        }

        public double BottomPosition
        {
            get { return bottomPosition; }
            set { bottomPosition = value; OnPropertyChanged("BottomPosition"); }
        }

        public double LeftPosition
        {
            get { return leftPosition; }
            set { leftPosition = value; OnPropertyChanged("LeftPosition"); }
        }

        public Brush BarColor
        {
            get { return barColor; }
            set { barColor = value; OnPropertyChanged("BarColor"); }
        }

        public double Value
        {
            get { return value; }
            set { this.value = value; OnPropertyChanged("Value"); }
        }

        public string LabelX
        {
            get { return labelX; }
            set { labelX = value; OnPropertyChanged("LabelX"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MeasurementGraphViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RoadEntity> AllRoads => MainWindowViewModel.Roads;

        private RoadEntity selectedRoad;
        public RoadEntity SelectedRoad
        {
            get { return selectedRoad; }
            set
            {
                if (selectedRoad != null)
                {
                    selectedRoad.PropertyChanged -= SelectedRoad_PropertyChanged;
                }

                selectedRoad = value;
                OnPropertyChanged("SelectedRoad");

                if (selectedRoad != null)
                {
                    selectedRoad.PropertyChanged += SelectedRoad_PropertyChanged;
                }

                OnPropertyChanged("AlarmLimitLabel");
                OnPropertyChanged("AlarmLimitHeight");

                RefreshGraph();
            }
        }

        public string AlarmLimitLabel
        {
            get
            {
                if (SelectedRoad?.Type == null) return "0 [ALARM]";
                return SelectedRoad.Type.Name == "IA" ? "15000 [ALARM]" : "7000 [ALARM]";
            }
        }

        // Računanje visine crvene linije alarma (Canvas.Bottom) na bazi G3 matematike
        public double AlarmLimitHeight
        {
            get
            {
                if (SelectedRoad?.Type == null) return 0;
                double limit = SelectedRoad.Type.Name == "IA" ? 15000.0 : 7000.0;

                // Maksimalni poluprečnik za max vrednost (21000) je 50px (krug 100x100px u Canvasu od 200px)
                double maxRadius = 50.0;
                double scaleFactor = maxRadius / 21000.0;
                double alarmRadius = limit * scaleFactor;

                // Centri su poravnati na Y=100 (od dna), pa alarm raste iz centra nagore za poluprečnik alarma
                return 100.0 + alarmRadius;
            }
        }

        public ObservableCollection<GraphBar> GraphBars { get; set; } = new ObservableCollection<GraphBar>();

        public MeasurementGraphViewModel()
        {
            for (int i = 0; i < 5; i++)
            {
                GraphBars.Add(new GraphBar { Height = 0, BottomPosition = 100, LeftPosition = 32, BarColor = Brushes.Transparent, Value = 0, LabelX = "--:--:--" });
            }
        }

        private void SelectedRoad_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshGraph();
                });
            }
        }

        public void RefreshGraph()
        {
            if (SelectedRoad == null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GraphBars[i].Height = 0;
                    GraphBars[i].BottomPosition = 100;
                    GraphBars[i].LeftPosition = 32;
                    GraphBars[i].BarColor = Brushes.Transparent;
                    GraphBars[i].Value = 0;
                    GraphBars[i].LabelX = "--:--:--";
                }
                return;
            }

            var history = SelectedRoad.History;

            // Maksimalni poluprečnik je 50px (Maksimalni prečnik kruga je 100px)
            double maxRadius = 50.0;
            double scaleFactor = maxRadius / 21000.0;

            OnPropertyChanged("AlarmLimitHeight");

            for (int i = 0; i < 5; i++)
            {
                if (i < history.Count)
                {
                    double val = history[i].Item1;
                    DateTime time = history[i].Item2;

                    // Računamo poluprečnik i prečnik za trenutnu vrednost
                    double radius = val * scaleFactor;
                    double diameter = radius * 2.0;

                    // ZAHTEV G3: Centri moraju biti poravnati paralelno po X-osi tačno na sredini (Y = 100 odozdo).
                    // Da bi centar kruga bio na 100, njegova donja ivica (Canvas.Bottom) se spušta za poluprečnik:
                    GraphBars[i].BottomPosition = 100.0 - radius;

                    // Centriranje kruga unutar širine slota (80px): leva pozicija je (80 - prečnik) / 2
                    GraphBars[i].LeftPosition = (80.0 - diameter) / 2.0;

                    GraphBars[i].Value = val;
                    GraphBars[i].Height = diameter; // Prečnik šaljemo u XAML za Width i Height
                    GraphBars[i].LabelX = time.ToString("HH:mm:ss");

                    bool isAlarm = false;
                    if (SelectedRoad.Type != null)
                    {
                        isAlarm = SelectedRoad.Type.Name == "IA" ? val > 15000.0 : val > 7000.0;
                    }

                    GraphBars[i].BarColor = isAlarm ?
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")) :
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                }
                else
                {
                    GraphBars[i].Height = 0;
                    GraphBars[i].BottomPosition = 100;
                    GraphBars[i].LeftPosition = 32;
                    GraphBars[i].BarColor = Brushes.Transparent;
                    GraphBars[i].Value = 0;
                    GraphBars[i].LabelX = "--:--:--";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}