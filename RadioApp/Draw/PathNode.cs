using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RadioApp.Draw
{
    public class PathNode : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        public double XPos { get; set; }
        public double YPos { get; set; }

        public Geometry? Geometry { get; set; }
        public string? Title { get; }
        public int Count { get; }
        public Brush? Fill { get; set; }
        public Brush? Stroke { get; set; }

        public PathNode(Geometry geometry, string? title, int? count)
        {
            XPos = 0;
            YPos = 0;

            Geometry = geometry;
            Title = title;
            Count = count ?? 0;

            //Fill = new SolidColorBrush(Colors.LightBlue);
            Fill = new SolidColorBrush(Colors.White);
            Stroke = new SolidColorBrush(Colors.Black);
        }
    }
}