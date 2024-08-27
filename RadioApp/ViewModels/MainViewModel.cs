using NAudio.Wave;
using Radio;
using Radio.Data;
using RadioApp.Core;
using RadioApp.Draw;
using RadioApp.Resources;
using RadioLib.Data;
using RadioLib.Image;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
//using System.Windows.Shapes;

namespace RadioApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        private AudioPlayer _player;
        private RadioClient _client;
        // <OnMap, InApi>
        private ILookup<string?, string?> _stateMapping;

        public ObservableCollection<State> States { get; set; }

        public ObservableCollection<StationGroup> Stations { get; set; }

        public ObservableCollection<PathNode> StatesGeometry { get; set; }

        public ICommand StateSelectionCommand { get; set; }
        public ICommand StationSelectionCommand { get; set; }
        public ICommand PlayCommand { get; set; }

        public string? SelectedStateName { get; set; } = string.Empty;
        public Station? SelectedStation { get; set; } = null;

        public bool IsLoading { get; set; } = false;
        public bool IsRunning => _player.IsPlaying;

        public byte[]? CurrentPage { get; set; }

        public MainViewModel()
        {
            _player = new AudioPlayer();
            _client = new RadioClient();

            _player.OnDataReceived += DataReceived;

            var states = _client.GetStates("Ukraine");
            if (states != null)
                States = new ObservableCollection<State>(states);
            else
                States = new ObservableCollection<State>();

            Stations = new ObservableCollection<StationGroup>();

            StatesGeometry = new ObservableCollection<PathNode>();
            InitGeometry();

            StateSelectionCommand = new SimpleCommand<PathNode>(StateSelection);
            StationSelectionCommand = new SimpleCommand<Station>(StationSelection);
            PlayCommand = new SimpleCommand(Play);
            //  var tst = svgPath.Title == null ? null : StatesMap.ResourceManager.GetString(svgPath.Title);

            //_stateMapping = new Lookup<string, string> { };

            var mapList = new List<(string? map, string? api)>();
            var allResources = StatesMap.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, false);

            if (allResources != null)
                foreach (DictionaryEntry keyValuePair in allResources)
                {
                    // TODO : add null-checks
                    mapList.Add((keyValuePair.Value?.ToString(), keyValuePair.Key.ToString()));
                }

            _stateMapping = mapList.ToLookup(key => key.map, val => val.api);
        }

        private void DataReceived(object? sender, byte[]? e)
        {
            CurrentPage = e;
            OnPropertyChanged("CurrentPage");
        }

        private void Play()
        {
            if (IsRunning)
                _player.Stop();
            else
                _player.Play();

            OnPropertyChanged("IsRunning");
        }

        private async void StateSelection(PathNode node)
        {
            IsLoading = true;
            OnPropertyChanged("IsLoading");

            try
            {
                var allApiStates = _stateMapping[node.Title];
                SelectedStateName = node.Title;

                OnPropertyChanged("SelectedStateName");

                Stations.Clear();
                foreach (var state in allApiStates)
                {
                    if (state == null)
                        continue;

                    var stationsInState = await _client.GetStationsAsync(state);
                    if (stationsInState == null)
                        continue;

                    foreach (var st in stationsInState.GroupBy(_ => _.HomePage))
                    {
                        if (st.Key == null)
                            continue;

                        Stations.Add(new StationGroup(st.Key, st.Select(_ => _)));
                    }
                }
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged("IsLoading");
            }
        }

        private void StationSelection(Station station)
        {
            SelectedStation = station;
            OnPropertyChanged("SelectedStation");

            // TODO : add UI response for cases without url
            if (station.Url != null)
                _player.SetUrl(station.Url);
            else
                _player.Stop();

            OnPropertyChanged("IsRunning");
        }

        private void InitGeometry()
        {
            var mapPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image\\ukraine.svg");
            var svgFile = SimpleSvg.Load(mapPath);

            foreach (var svgPath in svgFile.Paths)
            {
                var states = States.Where(_ => StatesMap.ResourceManager.GetString(_.Name) == svgPath.Title).ToList();
                var stationCount = States.Where(_ => StatesMap.ResourceManager.GetString(_.Name) == svgPath.Title).Sum(_ => _.StationCount);

                var pathNode = new PathNode(Geometry.Parse(svgPath.Path), svgPath.Title, stationCount ?? 0);

                StatesGeometry.Add(pathNode);
            }

            OnPropertyChanged("StatesGeometry");
        }
    }

    public class StationGroup
    {
        public List<Station> Stations { get; }

        public string HomePage { get; }

        public string? Title { get; }

        public string? Icon { get; }

        public StationGroup(string homepage, IEnumerable<Station> stations)
        {
            HomePage = homepage;
            Stations = new List<Station>(stations);

            var head = stations.FirstOrDefault();

            if (head != null)
            {
                Title = head.StationName;
                Icon = head.Icon;
            }
        }
    }
}