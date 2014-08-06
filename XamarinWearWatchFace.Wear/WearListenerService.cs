using System;
using XamarinWearFace.Common;
using Android.Content;
using Android.App;
using Java.Interop;
using Android.Gms.Wearable;

namespace WearXamFace
{
	[Service]
	[IntentFilter (new [] { "com.google.android.gms.wearable.BIND_LISTENER" })]
	public class WearListenerService : Android.Gms.Wearable.WearableListenerService
	{
		public static Settings Settings { get; set; }

		public static MainActivity WatchfaceActivity { get; set; }

		static WearListenerService ()
		{
			Settings = Settings.Load ();
		}

		public WearListenerService () : base ()
		{
		}

		public override void OnDataChanged (DataEventBuffer dataEvents)
		{
			Console.WriteLine ("Data Changed: " + dataEvents.Count);

			for (int i = 0; i < dataEvents.Count; i++) {

				var d = dataEvents.Get (i).JavaCast<IDataEvent> ();

				if (d.Type == DataEvent.TypeChanged) {

					try {
						var settings = Settings.Deserialize (d.DataItem.GetData ());
						Settings = settings;
						Settings.Save ();

						Console.WriteLine ("Saved Settings... " + settings);

						if (WatchfaceActivity != null) {
							try { WatchfaceActivity.UpdateSettings (); }
							catch { }
						}

					} catch (Exception ex) {
						Console.WriteLine ("Bad Message Data: " + ex);
					}
				}

			}
		}
	}
}

