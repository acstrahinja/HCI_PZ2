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

                RefreshGraph();
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
            double scaleFactor = 200.0 / 150.0;

            for (int i = 0; i < 5; i++)
            {
                if (i < history.Count)
                {
                    // Uzimamo vrednost (Item1) i vreme (Item2) iz Tuple-a
                    double val = history[i].Item1;
                    DateTime time = history[i].Item2;

                    GraphBars[i].Value = val;
                    GraphBars[i].Height = val * scaleFactor;
                    GraphBars[i].LabelX = time.ToString("HH:mm:ss"); // Formatirano vreme

                    GraphBars[i].BarColor = val > 100.0 ?
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")) :
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
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