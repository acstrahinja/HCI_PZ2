using System;

namespace NetworkService.Model
{
    public class RoadType
    {
        public string Name { get; set; }        // "IA" ili "IB"
        public string ImageSource { get; set; } // Putanja do slike na disku

        public RoadType() { }

        public RoadType(string name, string imageSource)
        {
            Name = name;
            ImageSource = imageSource;
        }
    }
}