﻿namespace XPlat.NuGet.UWP
{
    using System;
    using System.Collections.Generic;

    using Windows.Media.Playback;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    using XPlat.Device.Display;
    using XPlat.Device.Geolocation;
    using XPlat.Device.Power;
    using XPlat.Media.Capture;
    using XPlat.NuGet.UWP.Models;
    using XPlat.Storage;
    using XPlat.Storage.Pickers;

    public sealed partial class MainPage : Page
    {
        private Geolocator geolocator;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var request = new DisplayRequest();
            request.RequestActive();

            // Test with list of strings
            List<string> values = new List<string> { "Hello", "World", "ASDF" };
            ApplicationData.Current.LocalSettings.Values["Roles"] = values;

            // Test with list of objects
            List<Test> tests = new List<Test>
                                   {
                                       new Test
                                           {
                                               Date = DateTime.Now,
                                               Name = "Hello, World!",
                                               NestedTest =
                                                   new Test
                                                       {
                                                           Date = DateTime.MinValue,
                                                           Name = "Nested Hello!"
                                                       }
                                           },
                                       new Test
                                           {
                                               Date = DateTime.MinValue,
                                               Name = "Hello, World AGAIN!",
                                               NestedTest =
                                                   new Test
                                                       {
                                                           Date = DateTime.UtcNow,
                                                           Name = "Nested Hello AGAIN!"
                                                       }
                                           }
                                   };
            ApplicationData.Current.LocalSettings.Values["Tests"] = tests;

            var settings = ApplicationData.Current.LocalSettings.Values.Get<List<Test>>("Tests");

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                           "HelloWorld.txt",
                           CreationCollisionOption.OpenIfExists);

            await file.WriteTextAsync("Hello from the Android app!");

            var parent = await file.GetParentAsync();

            var batteryStatus = Device.Power.PowerManager.Current.BatteryStatus;
            var remainingPercentage = Device.Power.PowerManager.Current.RemainingChargePercent;

            Device.Power.PowerManager.Current.BatteryStatusChanged += this.PowerManager_BatteryStatusChanged;
            Device.Power.PowerManager.Current.RemainingChargePercentChanged +=
                this.PowerManager_RemainingChargePercentChanged;

            this.geolocator = new Geolocator()
                                  {
                                      DesiredAccuracy = PositionAccuracy.Default,
                                      MovementThreshold = 25
                                  };
            this.geolocator.PositionChanged += this.Geolocator_PositionChanged;

            var access = await this.geolocator.RequestAccessAsync();
            if (access == GeolocationAccessStatus.Allowed)
            {
                var currentLocation = await this.geolocator.GetGeopositionAsync(
                                          new TimeSpan(0, 0, 10),
                                          new TimeSpan(0, 0, 5));
                if (currentLocation != null)
                {
                    // ToDo
                }
            }

            CameraCaptureUI dialog = new CameraCaptureUI();
            dialog.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.Large3M;
            dialog.PhotoSettings.AllowCropping = false;

            IStorageFile capturedPhotoFile = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (capturedPhotoFile != null)
            {
                var parentFolder = await capturedPhotoFile.GetParentAsync();

                var copy = await capturedPhotoFile.CopyAsync(KnownFolders.CameraRoll);

                var props = await capturedPhotoFile.Properties.RetrievePropertiesAsync(null);

                var imageProps = await capturedPhotoFile.Properties.GetImagePropertiesAsync();

                var bytes = await capturedPhotoFile.ReadBytesAsync();
            }

            dialog.VideoSettings.MaxResolution = CameraCaptureUIMaxVideoResolution.HighestAvailable;
            dialog.VideoSettings.AllowTrimming = false;
            dialog.VideoSettings.MaxDurationInSeconds = 10;

            IStorageFile capturedVideoFile = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);

            if (capturedVideoFile != null)
            {
                var parentFolder = await capturedVideoFile.GetParentAsync();

                var copy = await capturedVideoFile.CopyAsync(KnownFolders.CameraRoll);

                var props = await capturedVideoFile.Properties.RetrievePropertiesAsync(null);

                var imageProps = await capturedVideoFile.Properties.GetImagePropertiesAsync();

                var bytes = await capturedVideoFile.ReadBytesAsync();
            }

            //var singleFilePick = new FileOpenPicker(this);
            //singleFilePick.FileTypeFilter.Add(".jpg");
            //var pickedFile = await singleFilePick.PickSingleFileAsync();

            //if (pickedFile != null)
            //{
            //    var imageProps = await pickedFile.Properties.GetImagePropertiesAsync();
            //}

            //var multiFilePick = new FileOpenPicker(this);
            //multiFilePick.FileTypeFilter.Add(".jpg");
            //var pickedFiles = await multiFilePick.PickMultipleFilesAsync();

            var fileData = await file.ReadTextAsync();

            request.RequestRelease();
        }

        private void PowerManager_RemainingChargePercentChanged(object sender, int e)
        {
            var percent = e;
        }

        private void PowerManager_BatteryStatusChanged(object sender, BatteryStatus e)
        {
            var status = e;
        }

        private void Geolocator_PositionChanged(IGeolocator sender, PositionChangedEventArgs args)
        {
            var position = args.Position;
        }
    }
}