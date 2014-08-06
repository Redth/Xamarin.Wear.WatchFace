using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;
using XamarinWearFace.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using System.Threading.Tasks;

namespace XamarinWatchFace
{
	[Activity (Label = "Xamarin Watch Face", MainLauncher = true, Icon = "@drawable/appiconaction")]
	public class MainActivity : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
	{

		IGoogleApiClient googleClient;

		SettingsFragment settingsFragment;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			RequestWindowFeature (WindowFeatures.ActionBar);


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			ActionBar.SetIcon (Resource.Drawable.appiconaction);


			settingsFragment = new SettingsFragment ();
			settingsFragment.OnSettingsChanged += Synchronize;

			FragmentManager.BeginTransaction ()
				.Replace (Resource.Id.fragment_container, settingsFragment)
				.Commit ();

			googleClient = new GoogleApiClientBuilder (this)
				.AddApi (WearableClass.Api)
				.AddConnectionCallbacks (this)
				.AddOnConnectionFailedListener (this)
				.Build ();

			googleClient.Connect ();
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			base.OnCreateOptionsMenu (menu);

			menu.Add ("Synchronize");

			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			Synchronize (settingsFragment.GetCurrentSettings ());

			return base.OnOptionsItemSelected (item);
		}


		void Synchronize (Settings settings)
		{
			SendCommand ("/SETTINGS", settingsFragment.GetCurrentSettings ().Serialize ());
		}


		public void OnConnected (Bundle connectionHint)
		{
			Console.WriteLine ("GoogleApi Google Wear Connected");

			//WearableClass.MessageApi.AddListener (googleClient, this);
		}

		public void OnConnectionSuspended (int cause)
		{
			Console.WriteLine ("GoogleApi Disconnected");
		}
		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{
			Console.WriteLine ("GoogleApi Connection Failed");
		}

		void SendCommand (string path, byte[] data = null)
		{
			if (data == null)
				data = new byte[] { 1 };

			Task.Factory.StartNew (() => {
				Console.WriteLine ("Wear DataApi -> Sending update in thread...");

				if (googleClient.IsConnected) {


					var pdr = PutDataRequest.Create ("/" + Guid.NewGuid().ToString ("D"));
					pdr.SetData (data);

					WearableClass.DataApi.PutDataItem (googleClient, pdr).Await ();

					Console.WriteLine ("Sent Data Item");
				}
			}).ContinueWith (t => {

				if (t.Exception != null) {
					Console.WriteLine ("Wear DataApi -> Failed to Send Contraction: " + t.Exception);
				}
			});
		}
	}
}


