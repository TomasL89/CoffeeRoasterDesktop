using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using Messages;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastViewModel : ITabViewModel, INotifyPropertyChanged, IDisposable
    {
        private const int PROFILE_ATTEMPT_LIMIT = 5;

        public string Name { get; } = "Roast";
        public string ImageSource { get; } = "/Resources/flame ( Kirill Kazachek).png";
        public string ErrorMessage { get; set; }
        public ICommand StartRoastCommand { get; }
        public ICommand StopRoastCommand { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand SendProfileToRoasterCommand { get; }
        public ICommand VerifyProfileCommand { get; }
        public ProfileService ProfileService { get; }
        public WpfPlot RoastPlot { get; set; }
        public RoastProfile RoastProfile { get; }
        public int CurrentTemperature { get; private set; }
        public int CurrentTime { get; private set; }
        public bool CanStartRoast { get; private set; }
        public bool ProfileIsValid { get; private set; }
        public string ProfileName { get; private set; }
        public string WiFiLastUpdated { get; private set; }
        public string TemperatureLastUpdated { get; private set; }
        public string ProgressLastUpdated { get; private set; }
        public string HeaterStatusLastUpdate { get; set; }

        private readonly IDisposable messageSubscription;

        //  private RoasterConnection roasterConnection;
        private int plotCounter = 0;

        private WpfPlot profilePlot;

        private readonly int maxPoints = 900;
        private readonly RoasterConnection roasterConnection;
        private readonly double[] data;
        private readonly double[] timeIntervals;
        private readonly List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>();
        private readonly List<RoastPoint> roastPoints = new List<RoastPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DataLogger dataLogger;

        public RoastViewModel(RoasterConnection roasterConnection)
        {
            this.roasterConnection = roasterConnection;
            ProfileService = new ProfileService();
            RoastProfile = new RoastProfile();
            RoastProfile = ProfileService.LoadProfile(@"C:\Users\Tom - Software Dev\Documents\testProfile.json");
            roastPoints = RoastProfile.RoastPoints;
            ProfileName = RoastProfile.RoastName;
            dataLogger = new DataLogger();
            data = new double[RoastProfile.RoastLengthTotalInSeconds];
            timeIntervals = new double[RoastProfile.RoastLengthTotalInSeconds];
            UpdateRoastPlotPoints();
            StartRoastCommand = new DelegateCommand(StartRoast);
            StopRoastCommand = new DelegateCommand(StopRoast);
            LoadProfileCommand = new DelegateCommand(LoadProfileFromFile);
            SendProfileToRoasterCommand = new DelegateCommand(SendProfile);
            VerifyProfileCommand = new DelegateCommand(GetProfile);
            // todo remove this, testing and dev only
            TemperatureLastUpdated = "10 seconds ago";
            WiFiLastUpdated = $"180 seconds ago";
            ProgressLastUpdated = "5 seconds ago";
            HeaterStatusLastUpdate = "25 seconds ago";

            messageSubscription = new CompositeDisposable()
            {
                roasterConnection.MessageRecieved.ObserveOnDispatcher().Do(UpdateData).Subscribe(),
                roasterConnection.WiFiConnected.ObserveOnDispatcher().Do(UpdateConnectionStatus).Subscribe()
            };

            //roasterConnection.ConnectToDevice();
            Random rand = new Random(0);
            for (var i = 0; i < maxPoints; i++)
            {
                timeIntervals[i] = i;
            }
            InitialisePlot();
        }

        private void LoadProfileFromFile()
        {
        }

        private void StopRoast()
        {
            roasterConnection.SendMessageToDevice("Stop");
            dataLogger.GetRoastById(roastId);
        }

        private void UpdateConnectionStatus(bool obj)
        {
        }

        private void GetProfile()
        {
            ProfileIsValid = ProfileIsValid ? false : true;
            // roasterConnection.SendMessageToDevice("Profile Get");
        }

        private void SendProfile()
        {
            try
            {
                for (var i = 0; i < PROFILE_ATTEMPT_LIMIT; i++)
                {
                    var profileMessage = ProfileService.GetProfileAsString(@"C:\Users\Tom - Software Dev\Documents\testProfile.json");
                    var result = roasterConnection.SendMessageToDeviceWithReply(@profileMessage + '\n');
                    if (result == null)
                        continue;

                    if (string.Equals(result, "valid_profile", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception)
            {
            }
        }

        private void StartRoast()
        {
            roastId = dataLogger.CreateLog();
            if (roastId != null)
            {
                CanStartRoast = true;
                roasterConnection.StartRoast();
                return;
            }
            ErrorMessage = "Unable to establish a connection to the database for logging";
            CanStartRoast = false;
        }

        // todo needs to be its own class
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
        }

        private void UpdateData(IMessage obj)
        {
            if (obj is TemperatureMessage temperatureMessage)
            {
                if (plotCounter >= maxPoints)
                    return;
                data[plotCounter] = temperatureMessage.Temperature;
                plotCounter += 1;

                dataLogger.SaveToDatabase(new RoastLog
                {
                    Temperature = temperatureMessage.Temperature,
                    TimeStamp = DateTime.UtcNow,
                    HeaterOn = false, //need to change
                    RoastDurationSecond = temperatureMessage.TimeInSeconds,
                    RoastId = roastId
                });

                UpdatePlot();
            }
        }

        private void UpdatePlot()
        {
            RoastPlot.Dispatcher.Invoke(new Action(() =>
            {
                var signalPlot = new WpfPlot();
                signalPlot.plt.PlotSignal(data, maxRenderIndex: plotCounter, color: System.Drawing.Color.Green, lineWidth: 2);
                RoastPlot.plt.Clear();
                RoastPlot.plt.Add(signalPlot.plt.GetPlottables().First());
                RoastPlot.plt.Add(profilePlot.plt.GetPlottables().First());
                RoastPlot.plt.Axis(x1: 0, x2: 900, y1: 0, y2: 210);

                foreach (var rp in roastPoints)
                {
                    RoastPlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.White, fontName: null, 16);
                }

                RoastPlot.Render();
            }));
        }

        private void InitialisePlot()
        {
            RoastPlot = new WpfPlot();
            RoastPlot.plt.Axis(x1: 0, x2: RoastProfile.RoastLengthTotalInSeconds, y1: 0, y2: 210);
            //RoastPlot.plt.Style(Style.Blue3);
            //RoastPlot.plt.Style()
            var profileXs = temperaturePlotPoints.Select(x => x.Item1).ToArray();
            var profileYs = temperaturePlotPoints.Select(x => x.Item2).ToArray();

            profilePlot = new WpfPlot();

            var bg = ConvertToColor(Styles.PlotStyle.ColormindWhite);
            var dg = ConvertToColor(Styles.PlotStyle.ColormindOrange);
            var gg = ConvertToColor(Styles.PlotStyle.ColormindGrey);
            var tg = ConvertToColor(Styles.PlotStyle.ColormindOrange);
            //profilePlot.plt.Style(bg, dg, gg, tg, System.Drawing.Color.Pink, System.Drawing.Color.Yellow);
            profilePlot.plt.PlotScatter(profileXs, profileYs, markerShape: MarkerShape.none, color: System.Drawing.Color.Red, lineWidth: 2);
            profilePlot.plt.Axis(x1: 0, x2: 900, y1: 0, y2: 210);
            foreach (var rp in roastPoints)
            {
                profilePlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.White, fontName: null, 16);
            }

            RoastPlot.plt.Add(profilePlot.plt.GetPlottables().First());
            RoastPlot.plt.Style(null, bg, gg, tg);
            RoastPlot.Dispatcher.Invoke(new Action(() =>
            {
                RoastPlot.Render();
            }));
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls
        private Guid roastId;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    messageSubscription.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public System.Drawing.Color ConvertToColor(SolidColorBrush brush)
        {
            return System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }

        #endregion IDisposable Support
    }
}