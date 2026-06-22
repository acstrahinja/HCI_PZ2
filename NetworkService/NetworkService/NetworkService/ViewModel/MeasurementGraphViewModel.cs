using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using NetworkService.Model;

namespace NetworkService.ViewModel
{
    public class GraphBar : INotifyPropertyChanged
    {
        private double height;
        private Brush barColor;
        private double value;
        private string labelX;

        public double Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
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

                // Kada korisnik klikne na drugi put, odmah javljamo XAML-u da promeni tekst i visinu linije alarma
                OnPropertyChanged("AlarmLimitLabel");
                OnPropertyChanged("AlarmLimitHeight");

                RefreshGraph();
            }
        }

        // NOVO: Vraća dinamički tekst za crvenu liniju (npr. "15000 [ALARM]" ili "7000 [ALARM]")
        public string AlarmLimitLabel
        {
            get
            {
                if (SelectedRoad?.Type == null) return "0 [ALARM]";
                return SelectedRoad.Type.Name == "IA" ? "15000 [ALARM]" : "7000 [ALARM]";
            }
        }

        // NOVO: Računa visinu na kojoj crvena linija treba da se iscrta na Canvasu (u pikselima)
        public double AlarmLimitHeight
        {
            get
            {
                if (SelectedRoad?.Type == null) return 0;
                double limit = SelectedRoad.Type.Name == "IA" ? 15000.0 : 7000.0;
                double scaleFactor = 200.0 / 21000.0; // Prilagođeno T3 skali
                return limit * scaleFactor;
            }
        }

        public ObservableCollection<GraphBar> GraphBars { get; set; } = new ObservableCollection<GraphBar>();

        public MeasurementGraphViewModel()
        {
            // Inicijalno postavljamo prazne crtice na X osu
            for (int i = 0; i < 5; i++)
            {
                GraphBars.Add(new GraphBar { Height = 0, BarColor = Brushes.Transparent, Value = 0, LabelX = "--:--:--" });
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
                    GraphBars[i].BarColor = Brushes.Transparent;
                    GraphBars[i].Value = 0;
                    GraphBars[i].LabelX = "--:--:--";
                }
                return;
            }

            var history = SelectedRoad.History;

            // Maksimalna visina prostora na grafu je 200px, a maksimalna vrednost T3 simulatora je 21000
            double scaleFactor = 200.0 / 21000.0;

            // Forsiramo osvežavanje visine linije alarma kada stignu novi podaci
            OnPropertyChanged("AlarmLimitHeight");

            for (int i = 0; i < 5; i++)
            {
                if (i < history.Count)
                {
                    double val = history[i].Item1;
                    DateTime time = history[i].Item2;

                    GraphBars[i].Value = val;
                    GraphBars[i].Height = val * scaleFactor;
                    GraphBars[i].LabelX = time.ToString("HH:mm:ss");

                    // ISPRAVLJENO PREMA SPECIFIKACIJI ZA T3: Provera alarma zavisi od tipa puta (IA ili IB)
                    bool isAlarm = false;
                    if (SelectedRoad.Type != null)
                    {
                        isAlarm = SelectedRoad.Type.Name == "IA" ? val > 15000.0 : val > 7000.0;
                    }

                    GraphBars[i].BarColor = isAlarm ?
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")) : // Crvena za alarm
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));  // Plava za normalno stanje
                }
                else
                {
                    GraphBars[i].Height = 0;
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