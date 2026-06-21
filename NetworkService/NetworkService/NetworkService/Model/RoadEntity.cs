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
        private bool isSelected; // Dodato polje za CheckBox

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
            get { return value; }
            set { OnPropertyChanged("Value"); } //?? value = value; OnPropertyChanged("Value");
        }

        // Dodato svojstvo za štikliranje u tabeli
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}