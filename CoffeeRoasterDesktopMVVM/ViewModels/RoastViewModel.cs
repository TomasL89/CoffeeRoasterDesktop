﻿using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using Messages;
using Microsoft.Win32;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public ICommand SaveReportCommand { get; }
        public ICommand LoadReportCommand { get; }
        public ICommand CloseLoadWindowCommand { get; }
        public ProfileService ProfileService { get; }
        public WpfPlot RoastPlot { get; set; }

        private string profileLocation;

        public RoastProfile RoastProfile { get; private set; }
        public int CurrentTemperature { get; private set; }
        public int CurrentTime { get; private set; }
        public bool CanStartRoast { get; private set; }
        public bool ProfileIsValid { get; private set; }
        public bool SaveRoastWindowEnabled { get; private set; } = true;

        public RoastReport RoastReport { get; set; }

        public string WiFiStrengthPercentage { get; set; }
        public string WiFiLastUpdated { get; private set; }

        public string BeanTemperature { get; private set; }
        public string TemperatureLastUpdated { get; private set; }

        public string ProgressPercentage { get; private set; }
        public string ProgressLastUpdated { get; private set; }

        public string HeaterStatus { get; private set; }
        public string HeaterStatusLastUpdate { get; private set; }

        public string FirstCrackTimeStampSeconds { get; private set; }

        private readonly IDisposable messageSubscription;

        //  private RoasterConnection roasterConnection;
        private int plotCounter = 0;

        private WpfPlot profilePlot;

        private readonly int maxPoints = 900;
        private readonly RoasterConnection roasterConnection;
        private readonly double[] data;
        private readonly double[] timeIntervals;
        private readonly List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>();
        private List<RoastPoint> roastPoints = new List<RoastPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DataLogger dataLogger;
        private readonly ReportService reportService;

        private DateTime wifiLastUpdate;
        private DateTime temperatureLastUpdate;
        private DateTime progressLastUpdate;
        private DateTime heaterStatusLastUpdate;

        public RoastViewModel(RoasterConnection roasterConnection)
        {
            this.roasterConnection = roasterConnection;
            ProfileService = new ProfileService();
            RoastProfile = new RoastProfile();
            RoastProfile = ProfileService.LoadProfile(@"C:\Users\Tom - Software Dev\Documents\testProfile.json");
            roastPoints = RoastProfile.RoastPoints;
            dataLogger = new DataLogger();
            reportService = new ReportService();
            RoastReport = new RoastReport();

            data = new double[RoastProfile.RoastLengthTotalInSeconds];
            timeIntervals = new double[RoastProfile.RoastLengthTotalInSeconds];
            UpdateRoastPlotPoints();

            StartRoastCommand = new DelegateCommand(StartRoast);
            StopRoastCommand = new DelegateCommand(StopRoast);
            LoadProfileCommand = new DelegateCommand(LoadProfileFromFile);
            SendProfileToRoasterCommand = new DelegateCommand(SendProfile);
            VerifyProfileCommand = new DelegateCommand(GetProfile);
            SaveReportCommand = new DelegateCommand(SaveReport);
            LoadReportCommand = new DelegateCommand(LoadReport);
            CloseLoadWindowCommand = new DelegateCommand(CloseLoadWindow);

            messageSubscription = new CompositeDisposable()
            {
                roasterConnection.MessageRecieved.ObserveOnDispatcher().Do(UpdateData).Subscribe(),
                roasterConnection.WifiConnectionChanged.ObserveOnDispatcher().Do(UpdateConnectionStatus).Subscribe()
            };

            //roasterConnection.ConnectToDevice();
            Random rand = new Random(0);
            for (var i = 0; i < maxPoints; i++)
            {
                timeIntervals[i] = i;
            }
            InitialisePlot();
        }

        private void CloseLoadWindow()
        {
            SaveRoastWindowEnabled = true;
        }

        private void LoadReport()
        {
            var reports = reportService.GetAllReports();
            SaveRoastWindowEnabled = false;
        }

        private void SaveReport()
        {
            reportService.SaveReportToDB(RoastReport);
        }

        private void LoadProfileFromFile()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Profile (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var roastProfile = ProfileService.LoadProfile(ofd.FileName);
                if (roastProfile != null)
                {
                    profileLocation = ofd.FileName;
                    RoastProfile = roastProfile;
                    roastPoints = RoastProfile.RoastPoints;
                    UpdateRoastPlotPoints();
                    InitialisePlot();
                }
            }
            //var allReports = reportService.GetAllReports();
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
            var reply = roasterConnection.SendMessageToDeviceWithReply("Profile Get");
            var profileFromDevice = ProfileService.ValidateRoastProfileAndDecode(reply);
            if (profileFromDevice != null)
            {
                RoastProfile = profileFromDevice;
                ProfileIsValid = true;
                roastPoints = RoastProfile.RoastPoints;
                UpdateRoastPlotPoints();
                InitialisePlot();
            }
        }

        private void SendProfile()
        {
            // todo this shouldn't be here, the timeouts and should be in the connection
            try
            {
                var profileMessage = ProfileService.GetProfileAsString(profileLocation);

                if (string.IsNullOrWhiteSpace(profileMessage))
                    return;

                var strippedProfileMessage = profileMessage.Replace("\r", "").Replace("\n", "");
                for (var i = 0; i < PROFILE_ATTEMPT_LIMIT; i++)
                {
                    var result = roasterConnection.SendMessageToDeviceWithReply(strippedProfileMessage + '\n');
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
            RoastPlot.plt.Clear();
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
                    HeaterOn = temperatureMessage.HeaterOn,
                    RoastDurationSecond = temperatureMessage.TimeInSeconds,
                    RoastId = roastId
                });
                BeanTemperature = $"{temperatureMessage.Temperature}  °C";
                ProgressPercentage = temperatureMessage.RoastProgress.ToString("0.00%");
                HeaterStatus = temperatureMessage.HeaterOn ? "On" : "Off";

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
                    RoastPlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.Black, fontName: null, 16);
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

            RoastPlot.plt.Add(profilePlot.plt.GetPlottables().First());
            RoastPlot.plt.Style(null, bg, gg, tg);
            RoastPlot.plt.Title(RoastProfile.RoastName);

            foreach (var rp in roastPoints)
            {
                RoastPlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.Black, fontName: null, 16);
            }

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