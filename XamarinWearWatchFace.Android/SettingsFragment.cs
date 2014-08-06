
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using XamarinWearFace.Common;

namespace XamarinWatchFace
{
	public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
	{
		Settings settings = null;

		public delegate void SettingsChangedDelegate (Settings settings);
		public event SettingsChangedDelegate OnSettingsChanged;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			AddPreferencesFromResource (Resource.Xml.preferences);
		}

		public override void OnResume ()
		{
			base.OnResume ();

			settings = Settings.Load ();

			PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener (this);
		}

		public override void OnPause ()
		{
			base.OnPause ();

			PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener (this);

			settings.Save ();
		}

		public void OnSharedPreferenceChanged (ISharedPreferences sharedPreferences, string key)
		{
			Console.WriteLine ("Preference Changed: " + key);

			switch (key) {
			case "prefshowxamarin":
				settings.ShowXamarinLogo = sharedPreferences.GetBoolean (key, true);
				break;
			case "pref24hourclock":
				settings.Use24Clock = sharedPreferences.GetBoolean (key, false);
				break;
			case "prefshowdayofweek":
				settings.ShowDayOfWeek = sharedPreferences.GetBoolean (key, true);
				break;
			case "prefshowdate":
				settings.ShowDate = sharedPreferences.GetBoolean (key, true);
				break;
			}

			settings.Save ();
			Console.WriteLine ("Settings> " + settings);

			var evt = OnSettingsChanged;
			if (evt != null)
				evt (settings);
		}

		public Settings GetCurrentSettings ()
		{
			return settings;
		}

	}
}

