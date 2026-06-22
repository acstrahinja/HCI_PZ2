using System;
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
                // Automatski javljamo tabeli da proveri promenu alarma
                OnPropertyChanged("IsInAlarm");
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        // GRANICA ALARMA: Ako je vrednost veća od 100, vraća true
        public bool IsInAlarm
        {
            get { return this.value > 100.0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}