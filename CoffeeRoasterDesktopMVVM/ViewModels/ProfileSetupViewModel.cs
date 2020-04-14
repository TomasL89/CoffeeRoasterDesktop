using Caliburn.Micro;
using CoffeeRoasterDesktopBackgroundLibrary;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class ProfileSetupViewModel : ITabViewModel, INotifyPropertyChanged
    {
        const int DefaultTimeInSeconds = 900;
        const int DefaultMaxTemperature = 250;
        const int DefaultMinTemperature = 30;
        const int DefaultDropDownHeight = 100;

        public int DropDownHeight { get; set; }
        public int RoastTimeInSeconds { get; set; } = DefaultTimeInSeconds;
        public int RoastTemperatureMax { get; set; } = DefaultMaxTemperature;
        public int RoastTemperatureMin { get; set; } = DefaultMinTemperature;
        public ObservableCollection<RoastProfilePointBase> RoastPointItems { get; set; } = new ObservableCollection<RoastProfilePointBase>();
        public List<SolidColorBrush> PhaseColours { get; set; } = new List<SolidColorBrush>();
        public SolidColorBrush SelectedPhaseColour { get; set; }
        public string Name { get; set; } = "Profile Setup";
        public RoastProfile RoastProfile { get; private set; }
        public ICommand AddNewRoastPointCommand { get; }
        public ICommand SaveRoastProfileCommand { get; }
        public ICommand LoadRoastProfileCommand { get; }
        public ICommand ConfigureProfileCommand { get; }
        public WpfPlot RoastProfilePlot { get; set; }


        private List<SolidColorBrush> colors = new List<SolidColorBrush>();
        private List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>(); 
        private int roastPointCounter;
        private List<RoastPoint> roastPoints = new List<RoastPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ProfileSetupViewModel()
        {
            RoastProfilePlot = new WpfPlot();
            RoastProfilePlot.plt.Axis(x1: 0, x2: RoastTimeInSeconds, y1: RoastTemperatureMin, y2: RoastTemperatureMax);
            RoastProfilePlot.plt.Style(Style.Blue3);
            RoastProfile = new RoastProfile();
            RoastPointItems.Add(new RoastPointButton());
            AddNewRoastPointCommand = new DelegateCommand(AddNewRoastPoint);
            SaveRoastProfileCommand = new DelegateCommand(SaveRoastProfile);
            LoadRoastProfileCommand = new DelegateCommand(LoadRoastProfile);
            ConfigureProfileCommand = new DelegateCommand(ConfigureProfile);
            //colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bfc693")));
            //colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8eb08a")));
            //colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#659784")));
            //colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477d7c")));
            //colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37626d")));
//#89D5E0, #C1DDCC, #E9E6BD, #F78E22, #FC7000, #9964F7, #A670DD, #2C897D, #F0A682, #AAFC8D, #EAF566, #AFFC98, #1BD0EE, #FFFB9F, #FFD6A8, #D2C7FF

            // todo add a way to parse in colors from the settings using the above string as input
            // these colors don't work too well with a white background and white text
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#89D5E0")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C1DDCC")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9E6BD")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F78E22")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC7000")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9964F7")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A670DD")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C897D")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0A682")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAFC8D")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAF566")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFFC98")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1BD0EE")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9F")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD6A8")));
            colors.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2C7FF")));
            PhaseColours.AddRange(colors);
        }

        private void ConfigureProfile()
        {

            DropDownHeight = DropDownHeight > 0 ? 0 : DefaultDropDownHeight;
        }

        private void LoadRoastProfile()
        {
            // todo implement RoastProfile Manager
        }

        private void SaveRoastProfile()
        {
            // todo implement RoastProfile Manager
        }

        private void RoastPointItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRoastPlotPoints();
        }

        private void UpdateRoastPlotPoints()
        {
            var priorTempSet = false;
            temperaturePlotPoints.Clear();
            double priorTemp = 0;
            double timeCounter = 0;
            foreach(var rp in roastPoints)
            {
                if(!priorTempSet)
                {
                    priorTempSet = true;
                    priorTemp = rp.Temperature;

                    for (var i = rp.StartSeconds; i < rp.EndSeconds ; i++)
                    {
                        temperaturePlotPoints.Add(new Tuple<double, double>(timeCounter, priorTemp));
                        timeCounter++;
                    }
                    continue;
                }

                var deltaTemperature = rp.Temperature - priorTemp;
                var deltaSeconds = rp.EndSeconds - rp.StartSeconds;
                if (deltaSeconds <= 0)
                    return;

                var temperatureStep = deltaTemperature / deltaSeconds;
                
                for(var j = rp.StartSeconds; j <= rp.EndSeconds; j++)
                {
                    priorTemp += temperatureStep;
                    temperaturePlotPoints.Add(new Tuple<double, double>(timeCounter, priorTemp));
                    timeCounter++;
                }
            }

            UpdateChart();
        }

        private void UpdateChart()
        {
            RoastProfilePlot.plt.Clear();
            var xS = temperaturePlotPoints.Select(x => x.Item1).ToArray();
            var yS = temperaturePlotPoints.Select(x => x.Item2).ToArray();

            RoastProfilePlot.plt.PlotScatter(xS, yS);
            //RoastProfilePlot.plt.Style(dataBg: System.Drawing.Color.FromArgb(0, 0, 0));
            //RoastProfilePlot.plt.Style(dataBg: System.Drawing.Color.FromArgb(0, 0, 0));
            RoastProfilePlot.plt.Style(Style.Blue3);
            // todo refactor when everything is working
            for (var i = 0; i < roastPoints.Count; i++)
            {
                var roastPoint = roastPoints[i];
                var brushColor = colors[i];
                // font color needs to be a single color and readable with the colors
                var roastPointColor = System.Drawing.Color.FromArgb(brushColor.Color.A, brushColor.Color.R, brushColor.Color.G, brushColor.Color.B);
                RoastProfilePlot.plt.PlotHSpan(roastPoints[i].StartSeconds, roastPoints[i].EndSeconds, roastPointColor);
                // this calculation needs to be a little better with accounting for the text length
                RoastProfilePlot.plt.PlotText(roastPoint.StageName, ((roastPoints[i].StartSeconds + roastPoints[i].EndSeconds)/2), 250, System.Drawing.Color.White, fontName:null, 16);

            }
            // todo fix this so it doesn't throw anything
            try
            {
               // RoastProfilePlot.plt.Grid(xSpacing: 30, ySpacing: 10);
                RoastProfilePlot.Render();
            }
            catch (Exception)
            {

            }
            
    
        }

        private void AddNewRoastPoint()
        {
            if (roastPointCounter >= colors.Count())
                return;

            var roastPoint = new RoastPoint
            {
                StageName = $"Stage {roastPointCounter}",
                StagePoint = roastPointCounter,
                Temperature = 30,
                StartSeconds = roastPoints.Count > 0 ? roastPoints.Last().EndSeconds : 0,
                EndSeconds = 0
            };

            var roastPointItem = new RoastPointItem()
            {
                Colour = colors[roastPointCounter],
                RoastPoint = roastPoint
            };

            RoastPointItems.Insert(roastPointCounter, roastPointItem);
            roastPoints.Add(roastPoint);
            roastPointCounter++;
            roastPoint.PropertyChanged += RoastPoint_PropertyChanged;
        }

        private void RoastPoint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRoastPlotPoints();
        }
    }
}
