using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware.Display;
using System.Threading;
using Android.Text;
using Android.Text.Style;

namespace WearXamFace
{
	[Activity (Label = "Xamarin", Theme="@android:style/Theme.DeviceDefault.NoActionBar", TaskAffinity="")]
	[MetaData ("com.google.android.clockwork.home.preview", Resource = "@drawable/preview")]
	[IntentFilter (new [] { Intent.ActionMain }, Categories = new [] { "com.google.android.clockwork.home.category.HOME_BACKGROUND" })]
	public class MainActivity : Activity, DisplayManager.IDisplayListener
	{
		bool isDimmed = false;
		DisplayManager displayManager;
		TextView time;
		TextView date;
		TextView day;
		ImageView xamarinLogo;
		Timer timerSeconds;
		ProgressWheel wheelSeconds;
		ProgressWheel wheelMinutes;
		ProgressWheel wheelHours;

		SimpleBroadcastReceiver timeBroadcastReceiver;
		SimpleBroadcastReceiver batteryBroadcastReceiver;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Console.WriteLine ("XamFace: OnCreate Called");

			displayManager = (DisplayManager)GetSystemService (Context.DisplayService);
			displayManager.RegisterDisplayListener (this, null);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);


			time = FindViewById<TextView> (Resource.Id.textTime);
			date = FindViewById<TextView> (Resource.Id.textDate);
			day = FindViewById<TextView> (Resource.Id.textDay);
			xamarinLogo = FindViewById<ImageView> (Resource.Id.xamarinLogo);
			wheelSeconds = FindViewById<ProgressWheel> (Resource.Id.wheelSeconds);
			wheelMinutes = FindViewById<ProgressWheel> (Resource.Id.wheelMinutes);
			wheelHours = FindViewById<ProgressWheel> (Resource.Id.wheelHours);

			wheelMinutes.Alpha = 0.6f;
			wheelHours.Alpha = 0.3f;
			wheelHours.ProgressMax = 24;

			wheelSeconds.RimColor = Android.Graphics.Color.Transparent;
			wheelHours.RimColor = Android.Graphics.Color.Transparent;
			wheelMinutes.RimColor = Android.Graphics.Color.Transparent;

			RunOnUiThread (UpdateUI);

			var filter = new IntentFilter ();
			filter.AddAction (Intent.ActionTimeTick);
			filter.AddAction (Intent.ActionTimeChanged);
			filter.AddAction (Intent.ActionTimezoneChanged);

			timeBroadcastReceiver = new SimpleBroadcastReceiver ();
			timeBroadcastReceiver.Receive = () => {
				Console.WriteLine ("Time Changed");
				RunOnUiThread (UpdateUI);
			};

			RegisterReceiver (timeBroadcastReceiver, filter);

			batteryBroadcastReceiver = new SimpleBroadcastReceiver ();
			batteryBroadcastReceiver.Receive = () => {
				Console.WriteLine ("Battery Changed");
				RunOnUiThread (UpdateUI);
			};

			RegisterReceiver (batteryBroadcastReceiver, new IntentFilter (Intent.ActionBatteryChanged));

			timerSeconds = new Timer (new TimerCallback (state => {
				RunOnUiThread (() => UpdateUITime (null));
			}), null, TimeSpan.FromSeconds (1), TimeSpan.FromSeconds (1));
		}

		protected override void OnNewIntent (Intent intent)
		{
			base.OnNewIntent (intent);

			Console.WriteLine ("New Intent");

			RunOnUiThread (UpdateUI);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			WearListenerService.WatchfaceActivity = this;
			UpdateUI ();
		}

		protected override void OnPause ()
		{
			WearListenerService.WatchfaceActivity = null;
			base.OnPause ();
		}
		void UpdateUI()
		{
			Console.WriteLine ("UpdateUI");

			var s = WearListenerService.Settings;
			if (s.ShowXamarinLogo != (xamarinLogo.Visibility == ViewStates.Visible))
				xamarinLogo.Visibility = s.ShowXamarinLogo ? ViewStates.Visible : ViewStates.Invisible;

			if (s.ShowDayOfWeek != (day.Visibility == ViewStates.Visible))
				day.Visibility = s.ShowDayOfWeek ? ViewStates.Visible : ViewStates.Invisible;

			if (s.ShowDate != (date.Visibility == ViewStates.Visible))
				date.Visibility = s.ShowDate ? ViewStates.Visible : ViewStates.Invisible;



			var dt = DateTime.UtcNow.ToLocalTime ();

			UpdateUITime (dt);

			date.Text = dt.ToString ("MMM d").ToUpperInvariant ();
			day.Text = dt.ToString ("dddd").ToUpperInvariant ();

			wheelMinutes.SetProgress (dt.Minute);
			wheelHours.SetProgress (dt.Hour);
		}

		void UpdateUITime (DateTime? date = null)
		{


			var dt = date.HasValue ? date.Value : DateTime.UtcNow.ToLocalTime ();

			wheelSeconds.SetProgress (dt.Second);

			//var fmt = "h:mm tt";
			var fmt = "h:mm";
			if (WearListenerService.Settings.Use24Clock)
				fmt = "H:mm";

			var str = dt.ToString (fmt).ToUpperInvariant ();

			var ss = new SpannableString (str);
			ss.SetSpan (new StyleSpan (Android.Graphics.TypefaceStyle.Bold), 0, str.IndexOf (':'), SpanTypes.ExclusiveExclusive);

			if (fmt.Contains (" ")) {
				var ttIndex = str.LastIndexOf (' ') + 1;

				ss.SetSpan (new ForegroundColorSpan (Android.Graphics.Color.Gray), ttIndex, ttIndex + 2, SpanTypes.ExclusiveExclusive);
				ss.SetSpan (new RelativeSizeSpan (0.8f), ttIndex, ttIndex + 2, SpanTypes.ExclusiveExclusive);
			}

			time.TextFormatted = ss;
		}

		public void OnDisplayAdded (int displayId)
		{
			Console.WriteLine ("Display Added");
		}

		public void OnDisplayChanged (int displayId)
		{
			Console.WriteLine ("Display Changed");

			switch(displayManager.GetDisplay (displayId).State) {
			case DisplayState.Dozing:
				Console.WriteLine ("Screen Dimming");
				isDimmed = true;
				timerSeconds.Change (Timeout.Infinite, Timeout.Infinite);
				wheelSeconds.Alpha = 0f;
				break;
			case DisplayState.Off:
				Console.WriteLine ("Screen Off");
				timerSeconds.Change (Timeout.Infinite, Timeout.Infinite);
				break;
			default:
				//  Not really sure what to so about Display.STATE_UNKNOWN, so
				//  we'll treat it as if the screen is normal.
				Console.WriteLine ("Screen Awake");
				isDimmed = false;
				wheelSeconds.Alpha = 1.0f;
				timerSeconds.Change (TimeSpan.FromSeconds (1), TimeSpan.FromSeconds (1));
				break;
			}

			RunOnUiThread (UpdateUI);
		}

		public void OnDisplayRemoved (int displayId)
		{
			timerSeconds.Change (Timeout.Infinite, Timeout.Infinite);
			Console.WriteLine ("Watch face removed");
		}

		public void UpdateSettings ()
		{
			Console.WriteLine ("Received Request to Update Settings");
			RunOnUiThread (UpdateUI);
		}
	}

	public class SimpleBroadcastReceiver : BroadcastReceiver
	{
		public Action Receive { get; set; }

		public override void OnReceive (Context context, Intent intent)
		{
			Console.WriteLine ("SimpleBroadcastReceiver: OnReceive");
			var tc = Receive;
			if (tc != null)
				tc ();
		}

	}

}


