using System;
using System.ComponentModel;
using System.IO;

using Android.Content;
using Android.Widget;
using ARelativeLayout = Android.Widget.RelativeLayout;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

[assembly: ExportRenderer(typeof(FormsVideoLibrary.VideoPlayer),
                          typeof(FormsVideoLibrary.Droid.VideoPlayerRenderer))]

namespace FormsVideoLibrary.Droid
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, ARelativeLayout>
    {
        VideoView videoView;
        MediaController mediaController;    // Used to display transport controls
        bool isPrepared;

        public VideoPlayerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    // Save the VideoView for future reference
                    videoView = new VideoView(Context);

                    // Put the VideoView in a RelativeLayout
                    ARelativeLayout relativeLayout = new ARelativeLayout(Context);
                    relativeLayout.AddView(videoView);

                    // Center the VideoView in the RelativeLayout
                    ARelativeLayout.LayoutParams layoutParams =
                        new ARelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    layoutParams.AddRule(LayoutRules.CenterInParent);
                    videoView.LayoutParameters = layoutParams;

                    // Handle a VideoView event
                    videoView.Prepared += OnVideoViewPrepared;

                    SetNativeControl(relativeLayout);
                }

                SetAreTransportControlsEnabled();
                SetSource();

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
            if (Control != null && videoView != null)
            {
                videoView.Prepared -= OnVideoViewPrepared;
            }
            if (Element != null)
            {
                Element.UpdateStatus -= OnUpdateStatus;
            }

            base.Dispose(disposing);
        }

        void OnVideoViewPrepared(object sender, EventArgs args)
        {
            isPrepared = true;
            ((IVideoPlayerController)Element).Duration = TimeSpan.FromMilliseconds(videoView.Duration);
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
            else if (args.PropertyName == VideoPlayer.PositionProperty.PropertyName)
            {
                if (Math.Abs(videoView.CurrentPosition - Element.Position.TotalMilliseconds) > 1000)
                {
                    videoView.SeekTo((int)Element.Position.TotalMilliseconds);
                }
            }
        }

        void SetAreTransportControlsEnabled()
        {
            if (Element.AreTransportControlsEnabled)
            {
                mediaController = new MediaController(Context);
                mediaController.SetMediaPlayer(videoView);
                videoView.SetMediaController(mediaController);
            }
            else
            {
                videoView.SetMediaController(null);

                if (mediaController != null)
                {
                    mediaController.SetMediaPlayer(null);
                    mediaController = null;
                }
            }
        }

        void SetSource()
        {
            isPrepared = false;
            bool hasSetSource = false;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    videoView.SetVideoPath(filename);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string package = Context.PackageName;
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string filename = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    string uri = "android.resource://" + package + "/raw/" + filename;
                    videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }

            if (hasSetSource && Element.AutoPlay)
            {
                videoView.Start();
            }
        }



        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            VideoStatus status = VideoStatus.NotReady;

            if (isPrepared)
            {
                status = videoView.IsPlaying ? VideoStatus.Playing : VideoStatus.Paused;
            }

            ((IVideoPlayerController)Element).Status = status;

            // Set Position property
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(videoView.CurrentPosition);
            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, timeSpan);
        }

        #region EVENTS
        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            videoView.Start();      
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            videoView.Pause();

        }

        void OnStopRequested(object sender, EventArgs args)
        {
            videoView.StopPlayback();
        }
        void OnRewindTenRequested(object sender, EventArgs args)
        {
            // Control.MediaPlayer.PlaybackSession.Position -= TimeSpan.FromSeconds(10);
            videoView.SeekTo(videoView.CurrentPosition - 10000);
        }

        void OnForwardTenRequested(object sender, EventArgs args)
        {
            //Control.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(10);
            videoView.SeekTo(videoView.CurrentPosition + 10000);
        }

        void OnLastFrameRequested(object sender, EventArgs args)
        {
            videoView.Pause();
            videoView.SeekTo(videoView.CurrentPosition - 500);
        }

        void OnNextFrameRequested(object sender, EventArgs args)
        {
            // Yendo frame por frame no renderiza bien y no muestra cada frame.
            //videoView.SeekTo(videoView.CurrentPosition + 42);
            videoView.Pause();
            videoView.SeekTo(videoView.CurrentPosition + 500);
        }

        void OnToggleSpeakerRequested(object sender, EventArgs args)
        {
            //Control.MediaPlayer.IsMuted = Control.MediaPlayer.IsMuted ? false : true;

        }

        void OnReduceSpeedRateRequested(object sender, EventArgs args)
        {
            // Control.MediaPlayer.PlaybackSession.PlaybackRate -= 0.25;
        }

        void OnIncreaseSpeedRateRequested(object sender, EventArgs args)
        {
            // Control.MediaPlayer.PlaybackSession.PlaybackRate += 0.25;
        }
        #endregion
    }
}