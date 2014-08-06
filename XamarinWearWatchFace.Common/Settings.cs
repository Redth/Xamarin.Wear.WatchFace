using System;
using System.IO;

namespace XamarinWearFace.Common
{
	public class Settings
	{
		public Settings ()
		{
			Use24Clock = false;
			ShowSecondsRing = true;
			ShowXamarinLogo = true;
			ShowDayOfWeek = true;
			ShowDate = true;

		}

		public bool Use24Clock { get;set; }
		public bool ShowSecondsRing { get;set; }
		public bool ShowXamarinLogo { get;set; }

		public bool ShowDayOfWeek { get; set; }
		public bool ShowDate { get; set; }

		public string SecondsRingColor { get;set; }
		public string MinutesRingColor { get;set; }
		public string HoursRingColor { get;set; }

		public string HoursColor { get;set; }
		public string MinutesColor { get;set; }

		public string DayOfWeekColor { get;set; }
		public string DateColor { get;set; }

		public byte[] Serialize ()
		{
			var json = Newtonsoft.Json.JsonConvert.SerializeObject (this);

			return System.Text.Encoding.UTF8.GetBytes (json);
		}

		public static Settings Deserialize (byte[] data)
		{
			var json = System.Text.Encoding.UTF8.GetString (data);

			return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings> (json);
		}

		public static Settings Load ()
		{
			Settings settings = null;

			try {
				var file = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "settings.json");

				var data = File.ReadAllText (file);

				settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings> (data);
			} catch {
			}

			return settings ?? new Settings ();
		}

		public void Save ()
		{
			var file = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "settings.json");

			try {
				var json = Newtonsoft.Json.JsonConvert.SerializeObject (this);

				File.WriteAllText (file, json);

			} catch { 
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Settings: Use24Clock={0}, ShowSecondsRing={1}, ShowXamarinLogo={2}, ShowDayOfWeek={3}, ShowDate={4}, SecondsRingColor={5}, MinutesRingColor={6}, HoursRingColor={7}, HoursColor={8}, MinutesColor={9}, DayOfWeekColor={10}, DateColor={11}]", Use24Clock, ShowSecondsRing, ShowXamarinLogo, ShowDayOfWeek, ShowDate, SecondsRingColor, MinutesRingColor, HoursRingColor, HoursColor, MinutesColor, DayOfWeekColor, DateColor);
		}
	}
}

