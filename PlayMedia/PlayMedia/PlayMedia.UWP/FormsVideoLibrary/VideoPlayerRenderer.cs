using System;
using System.ComponentModel;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Input;
using Windows.Foundation;

[assembly: ExportRenderer(typeof(FormsVideoLibrary.VideoPlayer),
                          typeof(FormsVideoLibrary.UWP.VideoPlayerRenderer))]

namespace FormsVideoLibrary.UWP
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, MediaPlayerElement>
    {
        Rect _sourceRect = new Rect(0, 0, 1, 1);

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    MediaPlayerElement _mediaPlayerElement = new MediaPlayerElement();
                    SetNativeControl(_mediaPlayerElement);

                    //MediaElement mediaElement = new MediaElement();
                    //SetNativeControl(mediaElement);

                    MediaPlayer _mediaPlayer = new MediaPlayer();
                    _mediaPlayerElement.SetMediaPlayer(_mediaPlayer);

                    _mediaPlayerElement.MediaPlayer.MediaOpened += OnMediaElementMediaOpened;
                    _mediaPlayerElement.MediaPlayer.CurrentStateChanged += OnMediaElementCurrentStateChanged;

                    // IsMuted
                    _mediaPlayerElement.MediaPlayer.IsMutedChanged += OnIsMutedChanged;

                    //Playback Rate
                    _mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackRateChanged += OnPlaybackRateChanged;

                    // Pinch and Zoom video
                    _mediaPlayerElement.ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                    _mediaPlayerElement.ManipulationDelta += OnManipulationDelta;
                    _mediaPlayerElement.DoubleTapped += OnDoubleTapped;


                    //mediaElement.MediaOpened += OnMediaElementMediaOpened;
                    //mediaElement.CurrentStateChanged += OnMediaElementCurrentStateChanged;
                }

                SetAreTransportControlsEnabled();
                SetSource();
                SetAutoPlay();

                args.NewElement.UpdateStatus += OnUpdateStatus;
                args.NewElement.PlayRequested += OnPlayRequested;
                args.NewElement.PauseRequested += OnPauseRequested;
                args.NewElement.StopRequested += OnStopRequested;
                args.NewElement.RewindTenRequested += OnRewindTenRequested;
                args.NewElement.ForwardTenRequested += OnForwardTenRequested;
                args.NewElement.LastFrameRequested += OnLastFrameRequested;
                args.NewElement.NextFrameRequested += OnNextFrameRequested;
                args.NewElement.ToggleSpeakerRequested += OnToggleSpeakerRequested;
                args.NewElement.ReduceSpeedRateRequested += OnReduceSpeedRateRequested;
                args.NewElement.IncreaseSpeedRateRequested += OnIncreaseSpeedRateRequested;
            }

            if (args.OldElement != null)
            {
                args.OldElement.UpdateStatus -= OnUpdateStatus;
                args.OldElement.PlayRequested -= OnPlayRequested;
                args.OldElement.PauseRequested -= OnPauseRequested;
                args.OldElement.StopRequested -= OnStopRequested;
                args.OldElement.RewindTenRequested -= OnRewindTenRequested;
                args.OldElement.ForwardTenRequested -= OnForwardTenRequested;
                args.OldElement.LastFrameRequested -= OnLastFrameRequested;
                args.OldElement.NextFrameRequested -= OnNextFrameRequested;
                args.OldElement.ToggleSpeakerRequested -= OnToggleSpeakerRequested;
                args.OldElement.ReduceSpeedRateRequested -= OnReduceSpeedRateRequested;
                args.OldElement.IncreaseSpeedRateRequested -= OnIncreaseSpeedRateRequested;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                Control.MediaPlayer.MediaOpened -= OnMediaElementMediaOpened;
                Control.MediaPlayer.CurrentStateChanged -= OnMediaElementCurrentStateChanged;
                Control.MediaPlayer.IsMutedChanged -= OnIsMutedChanged;
                Control.MediaPlayer.PlaybackSession.PlaybackRateChanged -= OnPlaybackRateChanged;
            }

            base.Dispose(disposing);
        }

        void OnMediaElementMediaOpened(MediaPlayer sender, object args)
        {
            ((IVideoPlayerController)Element).Duration = sender.PlaybackSession.NaturalDuration;
        }

        void OnMediaElementCurrentStateChanged(MediaPlayer sender, object args)
        {
            VideoStatus videoStatus = VideoStatus.NotReady;

            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    videoStatus = VideoStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    videoStatus = VideoStatus.Paused;
                    break;
                case MediaPlaybackState.None:
                    // Same as Stopped Status in deprecated(MediaElement)               
                    videoStatus = VideoStatus.NotReady;
                    break;
            }

            ((IVideoPlayerController)Element).Status = videoStatus;
        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VideoPlayer.AreTransportControlsEnabledProperty.PropertyName)
            {
                SetAreTransportControlsEnabled();
            }
            else if (args.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
            else if (args.PropertyName == VideoPlayer.AutoPlayProperty.PropertyName)
            {
                SetAutoPlay();
            }
            else if (args.PropertyName == VideoPlayer.PositionProperty.PropertyName)
            {
                if (Math.Abs((Control.MediaPlayer.PlaybackSession.Position - Element.Position).TotalSeconds) > 1)
                {
                    Control.MediaPlayer.PlaybackSession.Position = Element.Position;
                }
            }
        }


        void SetAreTransportControlsEnabled()
        {
            Control.AreTransportControlsEnabled = Element.AreTransportControlsEnabled;
        }

        void OnIsMutedChanged(MediaPlayer sender, object args)
        {
            Element.IsMuted = sender.IsMuted;
        }

        void OnPlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            Element.PlayBackRate = sender.PlaybackRate;
        }

        async void SetSource()
        {
            bool hasSetSource = false;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    Control.MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(uri));
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                // Code requires Pictures Library in Package.appxmanifest Capabilities to be enabled
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    StorageFile storageFile = await StorageFile.GetFileFromPathAsync(filename);
                    IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();
                    Control.MediaPlayer.Source = MediaSource.CreateFromStream(stream, storageFile.ContentType);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string path = "ms-appx:///" + (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    Control.Source = MediaSource.CreateFromUri(new Uri(path));
                    hasSetSource = true;
                }
            }

            if (!hasSetSource)
            {
                Control.Source = null;
            }
        }

        void SetAutoPlay()
        {
            Control.MediaPlayer.AutoPlay = Element.AutoPlay;
        }

        void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {           

            if (e.Delta.Scale != 1)
            {
                var halfWidth = _sourceRect.Width / 2;
                var halfHeight = _sourceRect.Height / 2;

                var centerX = _sourceRect.X + halfWidth;
                var centerY = _sourceRect.Y + halfHeight;

                var scale = e.Delta.Scale;
                var newHalfWidth = (_sourceRect.Width * e.Delta.Scale) / 2;
                var newHalfHeight = (_sourceRect.Height * e.Delta.Scale) / 2;

                if (centerX - newHalfWidth > 0 && centerX + newHalfWidth <= 1.0 &&
                    centerY - newHalfHeight > 0 && centerY + newHalfHeight <= 1.0)
                {
                    _sourceRect.X = centerX - newHalfWidth;
                    _sourceRect.Y = centerY - newHalfHeight;
                    _sourceRect.Width *= e.Delta.Scale;
                    _sourceRect.Height *= e.Delta.Scale;
                }
            }
            else
            {
                var translateX = -1 * e.Delta.Translation.X / Control.ActualWidth;
                var translateY = -1 * e.Delta.Translation.Y / Control.ActualHeight;

                if (_sourceRect.X + translateX >= 0 && _sourceRect.X + _sourceRect.Width + translateX <= 1.0 &&
                    _sourceRect.Y + translateY >= 0 && _sourceRect.Y + _sourceRect.Height + translateY <= 1.0)
                {
                    _sourceRect.X += translateX;
                    _sourceRect.Y += translateY;
                }
            }
            
            Control.MediaPlayer.PlaybackSession.NormalizedSourceRect = _sourceRect;
        }

        void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _sourceRect = new Rect(0, 0, 1, 1);
            
            Control.MediaPlayer.PlaybackSession.NormalizedSourceRect = _sourceRect;
        }

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, Control.MediaPlayer.PlaybackSession.Position);
        }

        #region EVENTS
        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            if (Control.MediaPlayer.Source == null)
            {
                SetSource();
            }
            Control.MediaPlayer.Play();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.Pause();
            Control.MediaPlayer.Source = null;

        }

        void OnRewindTenRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.PlaybackSession.Position -= TimeSpan.FromSeconds(10);
        }

        void OnForwardTenRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(10);
        }

        void OnLastFrameRequested(object sender, EventArgs args)
        {
            if (Control.MediaPlayer.Source != null)
            {
                Control.MediaPlayer.StepBackwardOneFrame();
            }
        }

        void OnNextFrameRequested(object sender, EventArgs args)
        {
            if (Control.MediaPlayer.Source == null)
            {
                SetSource();
            }
            Control.MediaPlayer.StepForwardOneFrame();
        }

        void OnToggleSpeakerRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.IsMuted = Control.MediaPlayer.IsMuted ? false : true;
        }

        void OnReduceSpeedRateRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.PlaybackSession.PlaybackRate -= 0.25;
        }

        void OnIncreaseSpeedRateRequested(object sender, EventArgs args)
        {
            Control.MediaPlayer.PlaybackSession.PlaybackRate += 0.25;
        }
        #endregion



    }
}