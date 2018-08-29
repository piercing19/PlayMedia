using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FormsVideoLibrary;

namespace PlayMedia
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CustomTransportExtendedPage : ContentPage
	{
		public CustomTransportExtendedPage ()
		{
			InitializeComponent ();
		}

        void OnPlayPauseButtonClicked(object sender, EventArgs args)
        {
            if (videoPlayer.Status == VideoStatus.Playing)
            {
                videoPlayer.Pause();
            }
            else if (videoPlayer.Status == VideoStatus.Paused)
            {
                videoPlayer.Play();
            }
        }

        void OnStopButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.Stop();
        }             

        void OnRewindButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.RewindTen();
        }

        void OnForwardButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.ForwardTen();
        }

        void OnLastFrameButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.LastFrame();
        }

        void OnNextFrameButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.NextFrame();
        }

        void OnToggleSpeakerButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.ToggleSpeaker();
        }

        void OnReduceSpeedRateButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.ReduceSpeedRate();
        }

        void OnIncreaseSpeedRateButtonClicked(object sender, EventArgs args)
        {
            videoPlayer.IncreaseSpeedRate();
        }

    }
}


     