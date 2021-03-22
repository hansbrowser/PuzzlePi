using System.IO;

namespace Worker.Models
{
	public class AppSettings
	{
		public string Resources { get; set; }

		public string Data { get; set; }

		public string Midi { get; set; }

		public string Image { get; set; }

		public string Game { get; set; }

		public string ImagePath { get { return Path.Combine(Directory.GetCurrentDirectory(), Resources, Data, Image); } }

		public string MidiPath { get { return Path.Combine(Directory.GetCurrentDirectory(), Resources, Data, Midi); } }

		public string GamePath { get { return Path.Combine(Directory.GetCurrentDirectory(), Resources, Data, Game); } }
	}
}