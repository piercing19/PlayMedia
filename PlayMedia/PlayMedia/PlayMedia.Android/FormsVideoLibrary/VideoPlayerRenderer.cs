using System;
using System.ComponentModel;
using System.IO;

using Android.Content;
using Android.Widget;
using ARelativeLayout = Android.Widget.RelativeLayout;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Views;
using Android.Media;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;

[assembly: ExportRenderer(typeof(FormsVideoLibrary.VideoPlayer),
                          typeof(FormsVideoLibrary.Droid.VideoPlayerRenderer))]

namespace FormsVideoLibrary.Droid
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, FrameLayout>, TextureView.ISurfaceTextureListener, ISurfaceHolderCallback
    {
        MediaPlayer _mediaPlayer;
        //VideoView videoView;
        MediaController mediaController;    // Used to display transport controls
        bool isPrepared;
        float _speedRate = 1.0f;
        PlaybackParams _playbackParams = new PlaybackParams();


        // New approach for older (VideoView) and new version (TextureView/SurfaceView) devices

        private FrameLayout _mainFrameLayout = null;

        private Android.Views.View _mainVideoView = null;

        public VideoPlayerRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {

                    // New Approach                    
                    _mainFrameLayout = new FrameLayout(Context);
                    _mediaPlayer = new MediaPlayer();

                    // We use VideoView for old versions
                    if (Build.VERSION.SdkInt < BuildVersionCodes.IceCreamSandwich)
                    {
                        Console.WriteLine("Using VideoView");
                        // Save the VideoView for future reference
                        var videoView = new VideoView(Context)
                        {
                            Background = new ColorDrawable(Xamarin.Forms.Color.Transparent.ToAndroid()),
                            Visibility = ViewStates.Gone,
                            LayoutParameters = new LayoutParams(
                            ViewGroup.LayoutParams.MatchParent,
                            ViewGroup.LayoutParams.MatchParent),
                        };

                        ISurfaceHolder holder = videoView.Holder;
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Honeycomb)
                        {
                            holder.SetType(SurfaceType.PushBuffers);
                        }
                        holder.AddCallback(this);
                        
                        _mainVideoView = videoView;
                    }
                    else
                    {
                        // We use TextureView for new versions
                        Console.WriteLine("Using TextureView");
                        // Save the TextureView for future reference
                        var textureView = new TextureView(Context);
                        //{
                        //    Background = new ColorDrawable(Xamarin.Forms.Color.Transparent.ToAndroid()),
                        //    Visibility = ViewStates.Gone,
                        //    LayoutParameters = new LayoutParams(
                        //    ViewGroup.LayoutParams.MatchParent,
                        //    ViewGroup.LayoutParams.MatchParent),
                        //};

                        textureView.SurfaceTextureListener = this;
                        
                        _mainVideoView = textureView;
                    }

                    // Main View Content Video
                    _mainFrameLayout.AddView(_mainVideoView);

                    // Handle a MediaPlayer event
                    _mediaPlayer.Prepared += OnVideoViewPrepared;
                    //_mediaPlayer.VideoSizeChanged += (sender, arg) =>
                    //{
                    //    AdjustTextureViewAspect(arg.Width, arg.Height);
                    //};

                    SetNativeControl(_mainFrameLayout);
                }

                SetAreTransportControlsEnabled();
                SetSource();

                // Subscribe
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
                // Unsubscribe
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
            if (Control != null && _mediaPlayer != null)
            {
                _mediaPlayer.Prepared -= OnVideoViewPrepared;
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
            ((IVideoPlayerController)Element).Duration = TimeSpan.FromMilliseconds(_mediaPlayer.Duration);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (Element == null || Control == null)
                return;

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
                if (Math.Abs(_mediaPlayer.CurrentPosition - Element.Position.TotalMilliseconds) > 1000)
                {
                    _mediaPlayer.SeekTo((int)Element.Position.TotalMilliseconds);
                }
            }
        }

        void SetAreTransportControlsEnabled()
        {
            if (Element.AreTransportControlsEnabled)
            {
                //mediaController = new MediaController(Context);
                //mediaController.SetMediaPlayer(videoView);
                //videoView.SetMediaController(mediaController);
            }
            else
            {
                //TODO if is a newer version or older version
                //videoView.SetMediaController(null);

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
                    _mediaPlayer.SetDataSource(Context, Android.Net.Uri.Parse(uri));
                    //videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    _mediaPlayer.SetDataSource(Context, Android.Net.Uri.Parse(filename));
                    //videoView.SetVideoPath(filename);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string package = Context.PackageName;
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    string uri = "android.resource://" + package + "/raw/" + filename;
                    _mediaPlayer.SetDataSource(Context, Android.Net.Uri.Parse(uri));
                    //videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }

            if (hasSetSource && Element.AutoPlay)
            {                
                _mediaPlayer.Prepare();
                _mediaPlayer.Start();
            }

        }


        //private void AdjustTextureViewAspect(int videoWidth, int videoHeight)
        //{
        //    if (!(_mainVideoView is TextureView))
        //        return;

        //    if (Control == null)
        //        return;

        //    var control = Control;

        //    var textureView = _mainVideoView as TextureView;

        //    var controlWidth = control.Width;
        //    var controlHeight = control.Height;
        //    var aspectRatio = (double)videoHeight / videoWidth;

        //    int newWidth, newHeight;

        //    if (controlHeight <= (int)(controlWidth * aspectRatio))
        //    {
        //        // limited by narrow width; restrict height  
        //        newWidth = controlWidth;
        //        newHeight = (int)(controlWidth * aspectRatio);
        //    }
        //    else
        //    {
        //        // limited by short height; restrict width  
        //        newWidth = (int)(controlHeight / aspectRatio);
        //        newHeight = controlHeight;
        //    }

        //    int xoff = (controlWidth - newWidth) / 2;
        //    int yoff = (controlHeight - newHeight) / 2;

        //    Console.WriteLine("video=" + videoWidth + "x" + videoHeight +
        //        " view=" + controlWidth + "x" + controlHeight +
        //        " newView=" + newWidth + "x" + newHeight +
        //        " off=" + xoff + "," + yoff);

        //    var txform = new Matrix();
        //    textureView.GetTransform(txform);
        //    txform.SetScale((float)newWidth / controlWidth, (float)newHeight / controlHeight);
        //    txform.PostTranslate(xoff, yoff);
        //    textureView.SetTransform(txform);
        //}



        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            VideoStatus status = VideoStatus.NotReady;

            if (isPrepared)
            {
                status = _mediaPlayer.IsPlaying ? VideoStatus.Playing : VideoStatus.Paused;
            }

            ((IVideoPlayerController)Element).Status = status;

            // Set Position property
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(_mediaPlayer.CurrentPosition);
            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, timeSpan);
        }

        #region Surface Texture Listener  

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            Console.WriteLine("Surface.TextureAvailable");
            _mediaPlayer.SetSurface(new Surface(surface));
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            Console.WriteLine("Surface.TextureDestroyed");
            return false;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            Console.WriteLine("Surface.TextureSizeChanged");
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            Console.WriteLine("Surface.TextureUpdated");
        }

        #endregion

        #region Surface Holder Callback  

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            Console.WriteLine("Surface.Changed");
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("Surface.Created");
            _mediaPlayer.SetDisplay(holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("Surface.Destroyed");
        }

        #endregion

        #region EVENTS
        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            _mediaPlayer.Start();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            _mediaPlayer.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            _mediaPlayer.Stop();
        }
        void OnRewindTenRequested(object sender, EventArgs args)
        {
            // Control.MediaPlayer.PlaybackSession.Position -= TimeSpan.FromSeconds(10);
            _mediaPlayer.SeekTo(_mediaPlayer.CurrentPosition - 10000);
        }

        void OnForwardTenRequested(object sender, EventArgs args)
        {
            //Control.MediaPlayer.PlaybackSession.Position += TimeSpan.FromSeconds(10);
            _mediaPlayer.SeekTo(_mediaPlayer.CurrentPosition + 10000);
        }

        void OnLastFrameRequested(object sender, EventArgs args)
        {
            _mediaPlayer.Pause();
            _mediaPlayer.SeekTo(_mediaPlayer.CurrentPosition - 500);
        }

        void OnNextFrameRequested(object sender, EventArgs args)
        {
            // Yendo frame por frame no renderiza bien y no muestra cada frame.
            //videoView.SeekTo(videoView.CurrentPosition + 42);
            _mediaPlayer.Pause();
            _mediaPlayer.SeekTo(_mediaPlayer.CurrentPosition + 500);            
        }

        void OnToggleSpeakerRequested(object sender, EventArgs args)
        {
            if (Element.IsMuted)
            {
                _mediaPlayer.SetVolume(1, 1);
                Element.IsMuted = false;
            }
            else {
                _mediaPlayer.SetVolume(0, 0);
                Element.IsMuted = true;
            }           
            
        }

        void OnReduceSpeedRateRequested(object sender, EventArgs args)
        {
            _speedRate -= 0.25f;
            Element.PlayBackRate = _speedRate;
            _playbackParams.SetSpeed(_speedRate);
            _mediaPlayer.PlaybackParams = _playbackParams;
            Element.PlayBackRate = _speedRate;
        }

        void OnIncreaseSpeedRateRequested(object sender, EventArgs args)
        {
            _speedRate += 0.25f;
            Element.PlayBackRate = _speedRate;
            _playbackParams.SetSpeed(_speedRate);
            _mediaPlayer.PlaybackParams = _playbackParams;
            
        }
        #endregion
    }
}