namespace Worker.Models
{
	public class MidiPlayFileModel
	{
		public string Filename { get; set; }

		public int Milliseconds { get; set; }

		public int OffSetMilliseconds { get; set; } = 0;
	}
}