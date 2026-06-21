using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetworkService.Model
{
    public class RoadEntity : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private RoadType type;
        private double val;

        public int ID
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public RoadType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public double Value
        {
            get { return val; }
            set
            {
                if (val != value)
                {
                    val = value;
                    OnPropertyChanged("Value");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        public bool IsValid
        {
            get
            {
                if (Type == null) return true;

                if (Type.Name == "IA")
                {
                    return Value <= 15000;
                }
                else if (Type.Name == "IB")
                {
                    return Value <= 7000;
                }

                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}