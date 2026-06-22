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
        private double value; // Tip ostaje double zbog kompatibilnosti sa ostatkom sistema i History listom
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

        // ISPRAVLJENO: Logika bez decimala i sa proverom tipa puta (IA / IB) koja rešava crvenilo na nuli
        public bool IsInAlarm
        {
            get
            {
                if (Type == null) return false;

                // Ako simulator još nije pokrenut (vrednost je 0), alarm je isključen
                if (this.value == 0) return false;

                // Ako je put IA reda, alarm je sve preko 15000
                if (Type.Name == "IA")
                {
                    return this.value > 15000;
                }
                // Ako je put IB reda, alarm je sve preko 7000
                else if (Type.Name == "IB")
                {
                    return this.value > 7000;
                }

                return false;
            }
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