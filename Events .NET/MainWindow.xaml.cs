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
            }
            
        }

        private void ReadFileToEvents(string path)
        {
            var lines = System.IO.File.ReadLines(path);
            var timestampColumnInfo = DetectTimestampColumn(lines);
            Events = lines.Select(line => new Event(line, timestampColumnInfo.Item1, timestampColumnInfo.Item2)).ToList();
            DrawChart();
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
            var bins = BinnedCounts(Events);

            EventsPlot = new PlotModel { PlotType = PlotType.XY };
            EventsPlot.Background = OxyColor.FromRgb(255, 255, 255);

            EventsPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = bins.Count,
                IsAxisVisible = true,
                Title = "Bin"
            });
            
            var series = new OxyPlot.Series.HistogramSeries();
            series.StrokeColor = OxyColor.FromRgb(0, 0, 100);
            series.StrokeThickness = 3.0;
            for(int i=0; i < bins.Count; i++)
            {
                series.Items.Add(new OxyPlot.Series.HistogramItem(i, i+1, bins[i].Item2, (int)bins[i].Item2, OxyColor.FromRgb(0,0,100)));
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
