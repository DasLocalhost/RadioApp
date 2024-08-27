using Radio;
using Radio.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RadioMaui.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        private RadioClient _client;

        public ObservableCollection<State> States { get; set; }

        public MainViewModel()
        {
            _client = new RadioClient();

            var states = _client.GetStates("Ukraine");
            if (states != null)
                States = new ObservableCollection<State>(states);
            else
                States = new ObservableCollection<State>();
        }
    }
}
