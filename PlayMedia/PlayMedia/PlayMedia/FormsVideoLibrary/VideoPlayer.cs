using Acr.UserDialogs;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace FormsVideoLibrary
{
    public class VideoPlayer : View, IVideoPlayerController
    {
        public event EventHandler UpdateStatus;

        public VideoPlayer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                UpdateStatus?.Invoke(this, EventArgs.Empty);
                return true;
            });
        }

        #region PROPERTIES
        // AreTransportControlsEnabled property
        public static readonly BindableProperty AreTransportControlsEnabledProperty =
            BindableProperty.Create(nameof(AreTransportControlsEnabled), typeof(bool), typeof(VideoPlayer), true);

        public bool AreTransportControlsEnabled
        {
            set { SetValue(AreTransportControlsEnabledProperty, value); }
            get { return (bool)GetValue(AreTransportControlsEnabledProperty); }
        }

        // IsMuted property
        public static readonly BindableProperty IsMutedProperty =
            BindableProperty.Create(nameof(IsMuted), typeof(bool), typeof(VideoPlayer), false);

        public bool IsMuted
        {
            set { SetValue(IsMutedProperty, value); }
            get { return (bool)GetValue(IsMutedProperty); }
        }

        // Source property
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(VideoSource), typeof(VideoPlayer), null);

        [TypeConverter(typeof(VideoSourceConverter))]
        public VideoSource Source
        {
            set { SetValue(SourceProperty, value); }
            get { return (VideoSource)GetValue(SourceProperty); }
        }

        // AutoPlay property
        public static readonly BindableProperty AutoPlayProperty =
            BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(VideoPlayer), true);

        public bool AutoPlay
        {
            set { SetValue(AutoPlayProperty, value); }
            get { return (bool)GetValue(AutoPlayProperty); }
        }
        
        // Status read-only property
        private static readonly BindablePropertyKey StatusPropertyKey =
            BindableProperty.CreateReadOnly(nameof(Status), typeof(VideoStatus), typeof(VideoPlayer), VideoStatus.NotReady);

        public static readonly BindableProperty StatusProperty = StatusPropertyKey.BindableProperty;

        public VideoStatus Status
        {
            get { return (VideoStatus)GetValue(StatusProperty); }
        }

        VideoStatus IVideoPlayerController.Status
        {
            set { SetValue(StatusPropertyKey, value); }
            get { return Status; }
        }

        // Duration read-only property
        private static readonly BindablePropertyKey DurationPropertyKey =
            BindableProperty.CreateReadOnly(nameof(Duration), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan(),
                propertyChanged: (bindable, oldValue, newValue) => ((VideoPlayer)bindable).SetTimeToEnd());

        public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
        }

        TimeSpan IVideoPlayerController.Duration
        {
            set { SetValue(DurationPropertyKey, value); }
            get { return Duration; }
        }

        // Position property
        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(nameof(Position), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan(),
                propertyChanged: (bindable, oldValue, newValue) => ((VideoPlayer)bindable).SetTimeToEnd());

        public TimeSpan Position
        {
            set { SetValue(PositionProperty, value); }
            get { return (TimeSpan)GetValue(PositionProperty); }
        }

        // Speed Rate property
        public static readonly BindableProperty PlayBackRateProperty =
            BindableProperty.Create(nameof(PlayBackRate), typeof(double), typeof(VideoPlayer), 1.0);

        public double PlayBackRate
        {
            set { SetValue(PlayBackRateProperty, value); }
            get { return (double)GetValue(PlayBackRateProperty); }
        }

        // TimeToEnd property
        private static readonly BindablePropertyKey TimeToEndPropertyKey =
            BindableProperty.CreateReadOnly(nameof(TimeToEnd), typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan());

        public static readonly BindableProperty TimeToEndProperty = TimeToEndPropertyKey.BindableProperty;

        public TimeSpan TimeToEnd
        {
            private set { SetValue(TimeToEndPropertyKey, value); }
            get { return (TimeSpan)GetValue(TimeToEndProperty); }
        }

        void SetTimeToEnd()
        {
            TimeToEnd = Duration - Position;
        }
        #endregion

        #region EVENTS
        // Methods handled by renderers
        public event EventHandler PlayRequested;

        public void Play()
        {
            Toast("Playing");
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PauseRequested;

        public void Pause()
        {
            Toast("Paused");
            PauseRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StopRequested;

        public void Stop()
        {
            Toast("Stopped");
            StopRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RewindTenRequested;

        public void RewindTen()
        {
            Toast("-10 seconds");
            RewindTenRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ForwardTenRequested;

        public void ForwardTen()
        {
            Toast("+10 seconds");
            ForwardTenRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LastFrameRequested;

        public void LastFrame()
        {
            Toast("Last frame");
            LastFrameRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler NextFrameRequested;

        public void NextFrame()
        {
            Toast("Next frame");
            NextFrameRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ToggleSpeakerRequested;

        public void ToggleSpeaker()
        {
            if (IsMuted)
            {
                Toast("Volume ON");
            }
            else
            {
                Toast("Volume OFF");
            }
            ToggleSpeakerRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ReduceSpeedRateRequested;

        public void ReduceSpeedRate()
        {
            Toast((PlayBackRate - 0.25) + "x Speed");
            ReduceSpeedRateRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IncreaseSpeedRateRequested;

        public void IncreaseSpeedRate()
        {
            Toast((PlayBackRate + 0.25) + "x Speed");
            IncreaseSpeedRateRequested?.Invoke(this, EventArgs.Empty);
        }     
        
        #endregion

        #region SETTINGS
        private static void Toast(string message)
        {
            ToastConfig toastConfig = new ToastConfig(message);
            toastConfig.SetDuration(750);
            toastConfig.SetBackgroundColor(Color.DimGray);

            UserDialogs.Instance.Toast(toastConfig);
        }
        #endregion

    }
}
