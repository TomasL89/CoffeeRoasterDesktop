using CoffeeRoasterDesktopBackgroundLibrary;
using CoffeeRoasterDesktopBackgroundLibrary.Data;
using CoffeeRoasterDesktopBackgroundLibrary.Error;
using CoffeeRoasterDesktopBackgroundLibrary.RoastProfile;
using CoffeeRoasterDesktopUI.Helpers;
using GalaSoft.MvvmLight.CommandWpf;
using LiteDB;
using Messages;
using Prism.Commands;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeRoasterDesktopUI.ViewModels
{
    public class RoastViewModel : ITabViewModel, INotifyPropertyChanged, IDisposable
    {
        private const int PROFILE_ATTEMPT_LIMIT = 5;
        private const int MAX_ROAST_TIME = 1200;
        private const string DEFAULT_IMAGE_ICON = "/Resources/photo (Good Ware).png";
        private const string DEFAULT_IMAGE_PREVIEW_ICON = "/Resources/warning (Freepik).png";
        private const string VALID_IMAGE_PREVIEW_ICON = "/Resources/valid photo (Good Ware).png";

        private string profileLocation;
        private int plotCounter = 0;
        private WpfPlot profilePlot;
        private readonly RoasterConnection roasterConnection;
        private double[] data;
        private readonly double[] timeIntervals;
        private List<Tuple<double, double>> temperaturePlotPoints = new List<Tuple<double, double>>();
        private List<RoastPoint> roastPoints = new List<RoastPoint>();
        private readonly DataLogger dataLogger;
        private readonly ReportService reportService;
        private DateTime wifiLastUpdate;
        private DateTime temperatureLastUpdate;
        private DateTime progressLastUpdate;
        private DateTime heaterStatusLastUpdate;
        private bool disposedValue = false; // To detect redundant calls
        private Guid roastId;
        private readonly IDisposable messageSubscription;
        private int firstCrackSeconds;

        public string Name { get; } = "Roast";
        public string ImageSource { get; } = "/Resources/flame ( Kirill Kazachek).png";
        public string ErrorMessage { get; set; }
        public ICommand StartRoastCommand { get; }
        public ICommand StopRoastCommand { get; }
        public ICommand HardwareTestCommand { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand SendProfileToRoasterCommand { get; }
        public ICommand VerifyProfileCommand { get; }
        public ICommand SaveReportCommand { get; }
        public ICommand LoadReportCommand { get; }
        public ICommand CloseLoadWindowCommand { get; }
        public ICommand DisplayReportDetailsCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand FirstCrackCommand { get; }
        public ProfileService ProfileService { get; }
        public WpfPlot RoastPlot { get; set; }
        public RoastProfile RoastProfile { get; private set; }
        public int CurrentTemperature { get; private set; }
        public int CurrentTime { get; private set; }
        public bool CanStartRoast { get; private set; }
        public bool ProfileIsValid { get; private set; } = true;
        public bool SaveRoastWindowEnabled { get; private set; } = true;
        public RoastReport RoastReport { get; set; }
        public ImageService ImageService { get; }
        public string WiFiStrength { get; set; }
        public string WiFiLastUpdated { get; private set; }
        public string BeanTemperature { get; private set; }
        public string TemperatureLastUpdated { get; private set; }
        public string ProgressPercentage { get; private set; }
        public string ProgressLastUpdated { get; private set; }
        public string HeaterStatus { get; private set; }
        public string HeaterStatusLastUpdate { get; private set; }
        public string FirstCrackTimeStampSeconds { get; private set; }
        public ObservableCollection<RoastReport> RoastReports { get; set; } = new ObservableCollection<RoastReport>();
        public BitmapImage ImageOnePreview { get; private set; } = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
        public string ImageOneIcon { get; private set; } = DEFAULT_IMAGE_ICON;
        public BitmapImage ImageTwoPreview { get; private set; } = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
        public string ImageTwoIcon { get; private set; } = DEFAULT_IMAGE_ICON;
        public BitmapImage ImageThreePreview { get; private set; } = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
        public string ImageThreeIcon { get; private set; } = DEFAULT_IMAGE_ICON;
        public BitmapImage ImageFourPreview { get; private set; } = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
        public string ImageFourIcon { get; private set; } = DEFAULT_IMAGE_ICON;

        public RoastReport SelectedRoastReport { get; set; }

        public List<Tuple<double, double>> RoastReportLogData = new List<Tuple<double, double>>();

        public event PropertyChangedEventHandler PropertyChanged;

        public RoastViewModel(RoasterConnection roasterConnection)
        {
            this.PropertyChanged += RoastViewModel_PropertyChanged;
            this.roasterConnection = roasterConnection;
            ProfileService = new ProfileService();
            RoastProfile = new RoastProfile();
            RoastProfile = ProfileService.LoadProfile(@"C:\Users\Tom - Software Dev\Documents\testProfile.json");
            roastPoints = RoastProfile.RoastPoints;
            dataLogger = new DataLogger();
            reportService = new ReportService();
            RoastReport = new RoastReport();
            ImageService = new ImageService();
            data = new double[MAX_ROAST_TIME];
            timeIntervals = new double[MAX_ROAST_TIME];

            for (var i = 0; i < MAX_ROAST_TIME; i++)
            {
                timeIntervals[i] = i;
            }
            UpdateRoastPlotPoints();

            StartRoastCommand = new DelegateCommand(StartRoast);
            StopRoastCommand = new DelegateCommand(StopRoast);
            HardwareTestCommand = new DelegateCommand(TestHardware);
            LoadProfileCommand = new DelegateCommand(LoadProfileFromFile);
            SendProfileToRoasterCommand = new DelegateCommand(SendProfile);
            VerifyProfileCommand = new DelegateCommand(GetProfile);
            SaveReportCommand = new DelegateCommand(SaveReport);
            LoadReportCommand = new DelegateCommand(LoadReport);
            CloseLoadWindowCommand = new DelegateCommand(CloseLoadWindow);
            DisplayReportDetailsCommand = new DelegateCommand(LoadReportDetails);
            AddImageCommand = new RelayCommand<int>(SaveImageToReport);
            FirstCrackCommand = new DelegateCommand(FirstCrack);

            messageSubscription = new CompositeDisposable()
            {
                roasterConnection.MessageRecieved.ObserveOnDispatcher().Do(UpdateData).Subscribe(),
                roasterConnection.WiFiConnectionChanged.ObserveOnDispatcher().Do(UpdateConnectionStatus).Subscribe(),
                roasterConnection.ProfileRecieved.ObserveOnDispatcher().Do(UpdateProfile).Subscribe(),
                roasterConnection.WiFiStrengthUpdated.ObserveOnDispatcher().Do(UpdateWiFiStrength).Subscribe()
            };
            InitialisePlot();
        }

        private void RoastViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedRoastReport))
            {
                RoastReport = SelectedRoastReport;
                LoadReportDetails();
            }
        }

        private void TestHardware()
        {
            plotCounter = 0;
            roasterConnection.SendMessageToDevice("Emulate");
        }

        private void UpdateWiFiStrength(string wifiStrength)
        {
            WiFiStrength = wifiStrength;
        }

        private void UpdateProfile(RoastProfile newProfile)
        {
            RoastProfile = newProfile;
            ProfileIsValid = true;
            roastPoints = RoastProfile.RoastPoints;
            UpdateRoastPlotPoints();
            InitialisePlot();
        }

        private void FirstCrack()
        {
            FirstCrackTimeStampSeconds = $"{plotCounter} s";
            firstCrackSeconds = plotCounter;
        }

        private void SaveImageToReport(int imageRef)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var roastImageId = ImageService.StoreRoastImage(ofd.FileName);

                if (roastImageId == Guid.Empty)
                    // todo warn user
                    return;

                switch (imageRef)
                {
                    case 1:
                        RoastReport.PhotoOneId = roastImageId;
                        ImageOnePreview = new BitmapImage(new Uri(ofd.FileName));
                        ImageOneIcon = VALID_IMAGE_PREVIEW_ICON;
                        break;

                    case 2:
                        RoastReport.PhotoTwoId = roastImageId;
                        ImageTwoPreview = new BitmapImage(new Uri(ofd.FileName));
                        ImageTwoIcon = VALID_IMAGE_PREVIEW_ICON;
                        break;

                    case 3:
                        RoastReport.PhotoThreeId = roastImageId;
                        ImageThreePreview = new BitmapImage(new Uri(ofd.FileName));
                        ImageThreeIcon = VALID_IMAGE_PREVIEW_ICON;
                        break;

                    case 4:
                        RoastReport.PhotoFourId = roastImageId;
                        ImageFourPreview = new BitmapImage(new Uri(ofd.FileName));
                        ImageFourIcon = VALID_IMAGE_PREVIEW_ICON;
                        break;

                    default:
                        break;
                }
            }
        }

        private void LoadReportDetails()
        {
            if (SelectedRoastReport == null)
                return;

            var logData = dataLogger.GetRoastById(SelectedRoastReport.RoastPlotId);

            if (logData.Count() == 0 || SelectedRoastReport.RoastPlotId == Guid.Empty)
                return;
            var temperatureLogData = logData.Select(x => x.Temperature).ToList();
            plotCounter = temperatureLogData.Count;
            RoastProfile = SelectedRoastReport.RoastProfile;
            roastPoints = RoastProfile.RoastPoints;
            UpdateRoastPlotPoints();
            for (var i = 0; i < plotCounter; i++)
            {
                if (i > MAX_ROAST_TIME)
                    break;

                data[i] = temperatureLogData[i];
            }

            if (RoastReport.PhotoOneId != Guid.Empty || RoastReport.PhotoOneId != null)
            {
                var image = LoadImageFromStream(RoastReport.PhotoOneId);
                if (image != null)
                {
                    ImageOnePreview = image;
                    ImageOneIcon = VALID_IMAGE_PREVIEW_ICON;
                }
            }

            if (RoastReport.PhotoTwoId != Guid.Empty || RoastReport.PhotoOneId != null)
            {
                var image = LoadImageFromStream(RoastReport.PhotoTwoId);
                if (image != null)
                {
                    ImageTwoPreview = image;
                    ImageTwoIcon = VALID_IMAGE_PREVIEW_ICON;
                }
            }

            if (RoastReport.PhotoThreeId != Guid.Empty || RoastReport.PhotoOneId != null)
            {
                var image = LoadImageFromStream(RoastReport.PhotoThreeId);
                if (image != null)
                {
                    ImageThreePreview = image;
                    ImageThreeIcon = VALID_IMAGE_PREVIEW_ICON;
                }
            }

            if (RoastReport.PhotoFourId != Guid.Empty || RoastReport.PhotoOneId != null)
            {
                var image = LoadImageFromStream(RoastReport.PhotoFourId);
                if (image != null)
                {
                    ImageFourPreview = image;
                    ImageFourIcon = VALID_IMAGE_PREVIEW_ICON;
                }
            }
            InitialisePlot();
            UpdatePlot();
        }

        private BitmapImage LoadImageFromStream(Guid photoId)
        {
            try
            {
                using (var stream = ImageService.Load(photoId))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void CloseLoadWindow()
        {
            SaveRoastWindowEnabled = true;
        }

        private void LoadReport()
        {
            var reports = reportService.GetAllReports();
            if (reports == null)
                return;
            SaveRoastWindowEnabled = false;
            RoastReports = new ObservableCollection<RoastReport>(reports);
            SelectedRoastReport = RoastReports.FirstOrDefault();

            RoastReport = SelectedRoastReport;

            if (SelectedRoastReport != null)
            {
                LoadReportDetails();
            }
        }

        private void SaveReport()
        {
            RoastReport.RoastProfile = RoastProfile;
            RoastReport.RoastPlotId = roastId;
            reportService.SaveReportToDB(RoastReport);
        }

        private void LoadProfileFromFile()
        {
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
                    profileLocation = ofd.FileName;
                    RoastProfile = roastProfile;
                    roastPoints = RoastProfile.RoastPoints;
                    UpdateRoastPlotPoints();
                    InitialisePlot();
                }
            }
        }

        private void StopRoast()
        {
            roasterConnection.SendMessageToDevice("Stop");
            dataLogger.GetRoastById(roastId);
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            if (!isConnected)
                WiFiStrength = "Disconnected";
            else if (isConnected && string.IsNullOrWhiteSpace(WiFiStrength))
                WiFiStrength = "Unknown";
        }

        private void GetProfile()
        {
            // todo fix this later
            roasterConnection.RequestProfileFromDevice();
        }

        private void SendProfile()
        {
            var profileMessage = ProfileService.GetProfileAsString(profileLocation);

            roasterConnection.SendProfileToRoaster(profileMessage, PROFILE_ATTEMPT_LIMIT);
        }

        private void StartRoast()
        {
            plotCounter = 0;
            RoastReport = new RoastReport();
            RoastPlot.plt.Clear();
            roastId = dataLogger.CreateLog();
            ImageOnePreview = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
            ImageOneIcon = DEFAULT_IMAGE_ICON;
            ImageTwoPreview = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
            ImageTwoIcon = DEFAULT_IMAGE_ICON;
            ImageThreePreview = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
            ImageThreeIcon = DEFAULT_IMAGE_ICON;
            ImageFourPreview = new BitmapImage(new Uri(DEFAULT_IMAGE_PREVIEW_ICON, UriKind.Relative));
            ImageFourIcon = DEFAULT_IMAGE_ICON;

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
            temperaturePlotPoints.Clear();

            temperaturePlotPoints = RoastPlotHelpers.UpdateTemperaturePlotPoints(roastPoints);
        }

        private void UpdateData(IMessage obj)
        {
            if (obj is TemperatureMessage temperatureMessage)
            {
                if (plotCounter >= temperaturePlotPoints.Count)
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
            if (plotCounter > temperaturePlotPoints.Count)
                return;
            try
            {
                RoastPlot.Dispatcher.Invoke(new Action(() =>
                {
                    var signalPlot = new WpfPlot();
                    signalPlot.plt.PlotSignal(data, maxRenderIndex: plotCounter, color: System.Drawing.Color.Green, lineWidth: 2);
                    RoastPlot.plt.Clear();
                    RoastPlot.plt.Add(signalPlot.plt.GetPlottables().First());
                    RoastPlot.plt.Add(profilePlot.plt.GetPlottables().First());
                    RoastPlot.plt.Axis(x1: 0, x2: MAX_ROAST_TIME, y1: 0, y2: 240);

                    foreach (var rp in roastPoints)
                    {
                        RoastPlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.Black, fontName: null, 16);
                    }

                    if (firstCrackSeconds > 0)
                        RoastPlot.plt.PlotText($"First Crack {firstCrackSeconds} s", firstCrackSeconds, data[firstCrackSeconds] + 5, System.Drawing.Color.Red, fontName: null, 14);

                    RoastPlot.Render();
                }));
            }
            catch (Exception ex)
            {
                ErrorService.LogError(SeverityLevel.Error, ErrorType.Plot, $"Class {typeof(RoastViewModel)} failed to render correctly.\n", ex);
            }
        }

        private void InitialisePlot()
        {
            RoastPlot = new WpfPlot();
            RoastPlot.plt.Axis(x1: 0, x2: MAX_ROAST_TIME, y1: 0, y2: 240);
            var profileXs = temperaturePlotPoints.Select(x => x.Item1).ToArray();
            var profileYs = temperaturePlotPoints.Select(x => x.Item2).ToArray();

            profilePlot = new WpfPlot();

            var bg = ColourHelper.ConvertToColor(Styles.PlotStyle.ColormindWhite);
            var dg = ColourHelper.ConvertToColor(Styles.PlotStyle.ColormindOrange);
            var gg = ColourHelper.ConvertToColor(Styles.PlotStyle.ColormindGrey);
            var tg = ColourHelper.ConvertToColor(Styles.PlotStyle.ColormindOrange);

            profilePlot.plt.PlotScatter(profileXs, profileYs, markerShape: MarkerShape.none, color: System.Drawing.Color.Red, lineWidth: 2);
            profilePlot.plt.Axis(x1: 0, x2: MAX_ROAST_TIME, y1: 0, y2: 240);

            RoastPlot.plt.Add(profilePlot.plt.GetPlottables().First());
            RoastPlot.plt.Style(null, bg, gg, tg);
            RoastPlot.plt.Title(RoastProfile.RoastName);

            foreach (var rp in roastPoints)
            {
                RoastPlot.plt.PlotText(rp.StageName, rp.EndSeconds + 1, rp.Temperature + 1, System.Drawing.Color.Black, fontName: null, 16);
            }

            RoastPlot.plt.Ticks(useExponentialNotation: false, useMultiplierNotation: false);

            RoastPlot.Dispatcher.Invoke(new Action(() =>
            {
                RoastPlot.Render();
            }));
        }

        #region IDisposable Support

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

        #endregion IDisposable Support
    }
}