using CoffeeRoasterDesktopBackgroundLibrary;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class ProfileSetupViewModel : ITabViewModel, INotifyPropertyChanged
    {
        private const int DefaultTimeInSeconds = 900;
        private const int DefaultMaxTemperature = 250;
        private const int DefaultMinTemperature = 30;
        private const int DefaultDropDownHeight = 100;
        private const int DefaultWindowWidth = 1800;

        public int DropDownHeight { get; set; }
        public int RoastTimeInSeconds { get; set; } = DefaultTimeInSeconds;
        public int RoastTemperatureMax { get; set; } = DefaultMaxTemperature;
        public int RoastTemperatureMin { get; set; } = DefaultMinTemperature;
        public ObservableCollection<RoastProfilePointBase> RoastPointItems { get; set; } = new ObservableCollection<RoastProfilePointBase>();

        public ObservableCollection<PhaseGroupBase> PhaseItems { get; } = new ObservableCollection<PhaseGroupBase>();

        public string Name { get; set; } = "Profile Setup";
        public string ImageSource { get; } = "/Resources/business (DinosoftLabs).png";
        public RoastProfile RoastProfile { get; private set; }
        public ICommand AddNewRoastPointCommand { get; }
        public ICommand SaveRoastProfileCommand { get; }
        public ICommand LoadRoastProfileCommand { get; }
        public ICommand ConfigureProfileCommand { get; }
        public ICommand AddNewPhaseGroupItemCommand { get; }
        public ProfileService ProfileService { get; }

        //  public ICommand AddPhaseGroupCommand { get; }
        public WpfPlot RoastProfilePlot { get; set; }

        public string PhaseName { get; set; }
        public Color SelectedColour { get; set; }

        private List<SolidColorBrush> colours = new List<SolidColorBrush>();
        private List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>();
        private int roastPointCounter;
        private List<RoastPoint> roastPoints = new List<RoastPoint>();
        private List<PhaseGroupItem> phaseItems = new List<PhaseGroupItem>();
        private int phaseIndexCounter;

        public event PropertyChangedEventHandler PropertyChanged;

        public ProfileSetupViewModel(RoasterConnection roasterConnection)
        {
            ProfileService = new ProfileService();
            RoastProfilePlot = new WpfPlot();
            RoastProfilePlot.plt.Axis(x1: 0, x2: RoastTimeInSeconds, y1: RoastTemperatureMin, y2: RoastTemperatureMax);
            RoastProfilePlot.plt.Style(Style.Blue3);
            RoastProfile = new RoastProfile();
            RoastPointItems.Add(new RoastPointButton());
            PhaseItems.Add(new PhaseGroupButton());
            AddNewRoastPointCommand = new DelegateCommand(AddNewRoastPoint);
            SaveRoastProfileCommand = new DelegateCommand(SaveRoastProfile);
            LoadRoastProfileCommand = new DelegateCommand(LoadRoastProfile);
            ConfigureProfileCommand = new DelegateCommand(ConfigureProfile);
            AddNewPhaseGroupItemCommand = new DelegateCommand(AddNewPhaseGroupItem);
            // AddPhaseGroupCommand = new DelegateCommand(AddPhaseGroupItem);
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bfc693")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8eb08a")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#659784")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477d7c")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37626d")));
            //#89D5E0, #C1DDCC, #E9E6BD, #F78E22, #FC7000, #9964F7, #A670DD, #2C897D, #F0A682, #AAFC8D, #EAF566, #AFFC98, #1BD0EE, #FFFB9F, #FFD6A8, #D2C7FF

            // todo add a way to parse in colors from the settings using the above string as input
            // these colors don't work too well with a white background and white text
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#89D5E0")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C1DDCC")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9E6BD")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F78E22")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FC7000")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9964F7")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A670DD")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C897D")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0A682")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAFC8D")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAF566")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFFC98")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1BD0EE")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFB9F")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD6A8")));
            //colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2C7FF")));
        }

        private void AddNewPhaseGroupItem()
        {
            if (phaseItems.Count >= colours.Count)
                return;

            var phaseGroupItem = new PhaseGroupItem
            {
                Colour = colours[phaseIndexCounter],
                PhaseStart = phaseItems.Count > 0 ? phaseItems.Last().PhaseEnd : 0,
                PhaseEnd = phaseItems.Count > 0 ? phaseItems.Last().PhaseEnd + 10 : 10,
                PhaseName = $"Phase Name {phaseIndexCounter}",
                PhaseRange = DefaultWindowWidth
            };

            phaseItems.Add(phaseGroupItem);
            var phaseButton = PhaseItems.Last();
            PhaseItems.Clear();
            PhaseItems.Add(phaseButton);
            var rebuildCounter = 0;
            foreach (var phaseItem in phaseItems)
            {
                phaseItem.PhaseRange = DefaultWindowWidth / phaseItems.Count;
                PhaseItems.Insert(rebuildCounter, phaseItem);
                rebuildCounter++;
            }

            //PhaseItems.Add(phaseGroupItem);
            //PhaseItems.Insert(phaseIndexCounter, phaseGroupItem);
            phaseIndexCounter += 1;
        }

        private void ConfigureProfile()
        {
            DropDownHeight = DropDownHeight > 0 ? 0 : DefaultDropDownHeight;
        }

        private void LoadRoastProfile()
        {
            // todo implement RoastProfile Manager
            var ofd = new OpenFileDialog();
            ofd.Filter = "Profile (*.json)|*.json";
            ofd.DefaultExt = ".json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var roastProfile = ProfileService.LoadProfile(ofd.FileName);
                if (roastProfile != null)
                {
                    roastPoints = roastProfile.RoastPoints;
                    UpdateRoastPlotPoints();
                }
            }
        }

        private void SaveRoastProfile()
        {
            // todo implement RoastProfile Manager
            var sfd = new SaveFileDialog();
            sfd.Filter = "Profile (*.json)|*.json";
            sfd.DefaultExt = ".json";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var roastProfile = new RoastProfile
                {
                    RoastLengthTotalInSeconds = RoastTimeInSeconds,
                    RoastName = new FileInfo(sfd.FileName).Name.Replace(".json", ""),
                    RoastPoints = roastPoints
                };

                ProfileService.SaveProfile(sfd.FileName, roastProfile);
            }
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
            foreach (var rp in roastPoints)
            {
                if (!priorTempSet)
                {
                    priorTempSet = true;
                    priorTemp = rp.Temperature;

                    for (var i = rp.StartSeconds; i < rp.EndSeconds; i++)
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

                for (var j = rp.StartSeconds; j <= rp.EndSeconds; j++)
                {
                    priorTemp += temperatureStep;
                    temperaturePlotPoints.Add(new Tuple<double, double>(timeCounter, priorTemp));
                    timeCounter++;
                }
            }

            UpdateChart();
        }

        //PhaseItem_SelectionChanged
        private void PhaseItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("Soooks");
        }

        private void UpdateChart()
        {
            RoastProfilePlot.plt.Clear();
            var xS = temperaturePlotPoints.Select(x => x.Item1).ToArray();
            var yS = temperaturePlotPoints.Select(x => x.Item2).ToArray();

            RoastProfilePlot.plt.PlotScatter(xS, yS);
            RoastProfilePlot.plt.Style(Style.Blue3);
            // todo refactor when everything is working

            for (var i = 0; i < roastPoints.Count; i++)
            {
                var roastPoint = roastPoints[i];
                // font color needs to be a single color and readable with the colors
                // var roastPointColor = System.Drawing.Color.FromArgb(brushColor.Color.A, brushColor.Color.R, brushColor.Color.G, brushColor.Color.B);
                // RoastProfilePlot.plt.PlotHSpan(roastPoints[i].StartSeconds, roastPoints[i].EndSeconds, roastPointColor);
                // this calculation needs to be a little better with accounting for the text length
                RoastProfilePlot.plt.PlotText(roastPoint.StageName, roastPoints[i].EndSeconds + 1, roastPoints[i].Temperature + 1, System.Drawing.Color.White, fontName: null, 16);
            }
            // todo fix this so it doesn't throw anything
            try
            {
                RoastProfilePlot.Render();
            }
            catch (Exception)
            {
            }
        }

        private void AddNewRoastPoint()
        {
            //if (roastPointCounter >= colours.Count())
            //    return;

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
                Colour = colours[0],
                RoastPoint = roastPoint,
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