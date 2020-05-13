using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.RoastProfile;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public int DropDownHeight { get; set; }
        public int RoastTimeInSeconds { get; set; } = DefaultTimeInSeconds;
        public int RoastTemperatureMax { get; set; } = DefaultMaxTemperature;
        public int RoastTemperatureMin { get; set; } = DefaultMinTemperature;
        public ObservableCollection<RoastProfilePointBase> RoastPointItems { get; set; } = new ObservableCollection<RoastProfilePointBase>();

        public string Name { get; } = "Profile Setup";
        public string ImageSource { get; } = "/Resources/business (DinosoftLabs).png";
        public RoastProfile RoastProfile { get; private set; }
        public ICommand AddNewRoastPointCommand { get; }
        public ICommand SaveRoastProfileCommand { get; }
        public ICommand LoadRoastProfileCommand { get; }
        public ICommand ConfigureProfileCommand { get; }
        public ProfileService ProfileService { get; }

        public WpfPlot RoastProfilePlot { get; set; }

        public string PhaseName { get; set; }
        public Color SelectedColour { get; set; }

        private List<SolidColorBrush> colours = new List<SolidColorBrush>();
        private List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>();
        private int roastPointCounter;
        private List<RoastPoint> roastPoints = new List<RoastPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ProfileSetupViewModel()
        {
            ProfileService = new ProfileService();
            RoastProfilePlot = new WpfPlot();
            RoastProfilePlot.plt.Axis(x1: 0, x2: RoastTimeInSeconds, y1: RoastTemperatureMin, y2: RoastTemperatureMax);
            RoastProfilePlot.plt.Style(Style.Blue3);
            RoastProfile = new RoastProfile();
            RoastPointItems.Add(new RoastPointButton());
            AddNewRoastPointCommand = new DelegateCommand(AddNewRoastPoint);
            SaveRoastProfileCommand = new DelegateCommand(SaveRoastProfile);
            LoadRoastProfileCommand = new DelegateCommand(LoadRoastProfile);
            ConfigureProfileCommand = new DelegateCommand(ConfigureProfile);

            // AddPhaseGroupCommand = new DelegateCommand(AddPhaseGroupItem);
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bfc693")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8eb08a")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#659784")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#477d7c")));
            colours.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37626d")));
        }

        private void ConfigureProfile()
        {
            DropDownHeight = DropDownHeight > 0 ? 0 : DefaultDropDownHeight;
        }

        private void LoadRoastProfile()
        {
            // todo implement RoastProfile Manager
            var ofd = new OpenFileDialog
            {
                Filter = "Profile (*.json)|*.json",
                DefaultExt = ".json"
            };

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
            var sfd = new SaveFileDialog
            {
                Filter = "Profile (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var roastProfile = new RoastProfile
                {
                    RoastLengthTotalInSeconds = RoastTimeInSeconds,
                    RoastName = new FileInfo(sfd.FileName).Name.Replace(".json", ""),
                    RoastPoints = roastPoints
                };

                var couldSaveProfile = ProfileService.SaveProfile(sfd.FileName, roastProfile);

                //  if (!couldSaveProfile)
                //display error to user
            }
        }

        private void RoastPointItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRoastPlotPoints();
        }

        private void UpdateRoastPlotPoints()
        {
            temperaturePlotPoints.Clear();

            temperaturePlotPoints = RoastPlotHelpers.UpdateTemperaturePlotPoints(roastPoints);

            UpdateChart();
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
                RoastProfilePlot.plt.PlotText(roastPoint.StageName, roastPoints[i].EndSeconds + 1, roastPoints[i].Temperature + 1, System.Drawing.Color.White, fontName: null, 16);
            }
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
            // todo this isn't great
            roastPoint.PropertyChanged += RoastPoint_PropertyChanged;
        }

        private void RoastPoint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRoastPlotPoints();
        }
    }
}