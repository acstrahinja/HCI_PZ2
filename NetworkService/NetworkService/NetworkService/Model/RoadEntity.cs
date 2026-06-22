using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NetworkService.Model
{
    public class RoadEntity : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private RoadType type;
        private double value;
        private bool isSelected;

        // Istorija čuva parove: stavka 1 je izmerena vrednost (double), stavka 2 je vreme merenja (DateTime)
        public List<Tuple<double, DateTime>> History { get; set; } = new List<Tuple<double, DateTime>>();

        public int ID
        {
            get { return id; }
            set { id = value; OnPropertyChanged("ID"); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        public RoadType Type
        {
            get { return type; }
            set { type = value; OnPropertyChanged("Type"); }
        }

        public double Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("IsInAlarm");

                // Prilikom svakog novog merenja sa simulatora, prosleđujemo vrednost i trenutno vreme
                AddToHistory(value, DateTime.Now);
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public bool IsInAlarm
        {
            get { return this.value > 100.0; }
        }

        // Pomoćna metoda koja osigurava da imamo najviše 5 zapisa u istoriji
        private void AddToHistory(double newValue, DateTime timestamp)
        {
            History.Add(new Tuple<double, DateTime>(newValue, timestamp));
            if (History.Count > 5)
            {
                History.RemoveAt(0); // Uklanja najstariji zapis (FIFO princip)
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}