using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;

namespace Events.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public List<Event> Events { get; set; }
        public PlotModel EventsPlot {get; private set; }

        private Property approach = new Property();
        public Property Approach
        {
            get { return approach; }
            set { approach = value; OnPropertyChanged(); }
        }
        
        private Property exit = new Property();

        public Property Exit
        {
            get { return exit; }
            set { exit = value; OnPropertyChanged(); }
        }

        private Property objectType = new Property();
        public Property ObjectType
        {
            get { return objectType; }
            set { objectType = value; OnPropertyChanged(); }
        }

        private Property movementType = new Property();
        public Property MovementType
        {
            get { return movementType; }
            set { movementType = value; OnPropertyChanged(); }
        }

        private string selectedApproach = "";

        public string SelectedApproach
        {
            get { return selectedApproach; }
            set { if (value != selectedApproach) { 
                    selectedApproach = value; OnPropertyChanged();
                    DrawChart();
                } 
            }
        }
        
        private string selectedExit = "";
        public string SelectedExit
        {
            get { return selectedExit; }
            set { if (value != selectedExit) 
                { 
                    selectedExit = value; OnPropertyChanged();
                    DrawChart();
                }
            }
        }

        private string selectedObjectType = "";
        public string SelectedObjectType
        {
            get { return selectedObjectType; }
            set { if (value != selectedObjectType) 
                { 
                    selectedObjectType = value; OnPropertyChanged();
                    DrawChart();
                }
            }
        }

        private string selectedMovementType = "";
        public string SelectedMovementType
        {
            get { return selectedMovementType; }
            set { if (value != selectedMovementType) 
                { 
                    selectedMovementType = value; OnPropertyChanged();
                    DrawChart();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                ReadFileToEvents(files[0]);
                ReadProperties();
            }
            
        }

        private void ReadFileToEvents(string path)
        {
            var lines = System.IO.File.ReadLines(path);
            var timestampColumnInfo = DetectTimestampColumn(lines);
            Events = lines.Select(line => new Event(line, timestampColumnInfo.Item1, timestampColumnInfo.Item2)).ToList();
            DrawChart();
        }

        private void ReadProperties()
        { 
            foreach(var e in Events)
            {
                var approachStr = e.Columns[0];
                var exitStr = e.Columns[1];
                var objectTypeStr = e.Columns[2];
                var movementTypeStr = e.Columns[3];

                if(!Approach.Values.Contains(approachStr))
                {
                    Approach.Values.Add(approachStr);
                }

                if (!Exit.Values.Contains(exitStr))
                {
                    Exit.Values.Add(exitStr);
                }

                if (!ObjectType.Values.Contains(objectTypeStr))
                {
                    ObjectType.Values.Add(objectTypeStr);
                }

                if (!MovementType.Values.Contains(movementTypeStr))
                {
                    MovementType.Values.Add(movementTypeStr);
                }
            }
        }

        private Tuple<int,Cliver.DateTimeRoutines.DateTimeFormat> DetectTimestampColumn(IEnumerable<string> lines)
        {
            var line = lines.First();
            var split = line.Split(',');

            for (int i=0;i<split.Length;i++)
            {
                DateTime dt;
                var success_UK = Cliver.DateTimeRoutines.TryParseDateTime(split[i], Cliver.DateTimeRoutines.DateTimeFormat.UK_DATE, out dt);
                var success_USA = Cliver.DateTimeRoutines.TryParseDateTime(split[i], Cliver.DateTimeRoutines.DateTimeFormat.USA_DATE, out dt);
                
                if (success_UK)
                {
                    return new Tuple<int, Cliver.DateTimeRoutines.DateTimeFormat>(i, Cliver.DateTimeRoutines.DateTimeFormat.UK_DATE);
                }
                else if (success_USA)
                {
                    return new Tuple<int, Cliver.DateTimeRoutines.DateTimeFormat>(i, Cliver.DateTimeRoutines.DateTimeFormat.USA_DATE);
                }
            }

            throw new Exception("Timestamp not found in any column.");
        }

        public void DrawChart()
        {
            var filteredEvents = Events;
            if(SelectedApproach != "")
            {
                filteredEvents = filteredEvents.Where(e => e.Columns[0] == SelectedApproach).ToList();
            }
            if (SelectedExit != "")
            {
                filteredEvents = filteredEvents.Where(e => e.Columns[1] == SelectedExit).ToList();
            }
            if (SelectedObjectType != "")
            {
                filteredEvents = filteredEvents.Where(e => e.Columns[2] == SelectedObjectType).ToList();
            }
            if (SelectedMovementType != "")
            {
                filteredEvents = filteredEvents.Where(e => e.Columns[3] == SelectedMovementType).ToList();
            }
            var bins = BinnedCounts(filteredEvents);

            EventsPlot = new PlotModel { PlotType = PlotType.XY };
            EventsPlot.Background = OxyColor.FromRgb(255, 255, 255);

            EventsPlot.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = DateTimeAxis.ToDouble(bins.First().Item1),
                Maximum = DateTimeAxis.ToDouble(bins.Last().Item1 + TimeSpan.FromMinutes(15)),
                IsAxisVisible = true,
                Title = "Bin"
            });
            
            var series = new OxyPlot.Series.HistogramSeries();
            series.StrokeColor = OxyColor.FromRgb(0, 0, 100);
            series.StrokeThickness = 3.0;
            for(int i=0; i < bins.Count; i++)
            {
                var width = DateTimeAxis.ToDouble(bins[i].Item1 + TimeSpan.FromMinutes(15)) - DateTimeAxis.ToDouble(bins[i].Item1);
                var item = new OxyPlot.Series.HistogramItem(DateTimeAxis.ToDouble(bins[i].Item1), DateTimeAxis.ToDouble(bins[i].Item1 + TimeSpan.FromMinutes(15)), bins[i].Item2*width, (int)bins[i].Item2, OxyColor.FromRgb(0, 0, 100));
                series.Items.Add(item);
            }
            EventsPlot.Series.Add(series);

            var maxBinCount = bins.Select(b => b.Item2).Max();

            EventsPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Bin-count",
                IsZoomEnabled = true, 
                Minimum = 0,
                Maximum = maxBinCount
            });

            OnPropertyChanged("EventsPlot");
        }

        private List<Tuple<DateTime,int>> BinnedCounts(List<Event> events)
        {
            var counts = new List<Tuple<DateTime,int>>();

            var startTime = events.First().Timestamp;
            var endTime = events.Last().Timestamp;

            int nBins = (int) Math.Ceiling( (endTime - startTime) / TimeSpan.FromMinutes(15) );

            var binStart = startTime;
            var binEnd = startTime + TimeSpan.FromMinutes(15);

            for(int i=0;i<nBins;i++)
            {
                var count = events.Where(e => e.Timestamp >= binStart && e.Timestamp < binEnd).Count();
                var t = new Tuple<DateTime, int>(binStart, count);
                counts.Add(t);

                binStart = binStart + TimeSpan.FromMinutes(15);
                binEnd = binStart + TimeSpan.FromMinutes(15);
            }


            return counts;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
