using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Worker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Worker.Services
{
	public class MidiService
	{
		private readonly ILogger<MidiService> _logger;
		private readonly IMidiOutput _output;
		private readonly AppSettings _settings;

		public MidiService(ILogger<MidiService> logger, IOptions<AppSettings> settings)
		{
			_settings = settings.Value;
			_logger = logger;
			_logger.LogInformation("### MidiService Started ###");

			var access = MidiAccessManager.Default;
			var test = access.Outputs?.FirstOrDefault()?.Id;

			// var outputClient = "128_0"; // access.Outputs.First().Id;
			_logger.LogInformation(test);
			try
			{
				_output = access.OpenOutputAsync(test).Result;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "No midi output found");
			}

			// Create directory if not exist
			Directory.CreateDirectory(_settings.MidiPath);
		}

		public void PlaySound()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_logger.LogInformation("### MidiService PlaySound ###");

				// var access = MidiAccessManager.Default;

				// var outputClient = "128_0"; // access.Outputs.First().Id;

				// _logger.LogInformation(_output);
				// var output = access.OpenOutputAsync(outputClient).Result;
				_output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.AcousticGrandPiano }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, 60, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
				Task.Delay(2000).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 60, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.Program, 0x30 }, 0, 2, 0); // Strings Ensemble
				_output.Send(new byte[] { 0x90, 62, 70 }, 0, 3, 0);
				Task.Delay(2000).Wait();
				_output.Send(new byte[] { 0x80, 62, 70 }, 0, 3, 0);
				//_output.CloseAsync();
				//_logger.LogInformation(outputClient);
				// _output = access.OpenOutputAsync(outputClient).Result;
				_output.Send(new byte[] { 0xC0, 9 }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, 0x40, 0x70 }, 0, 3, 0); // There are constant fields for each MIDI event
				System.Threading.Tasks.Task.Delay(2000).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 0x40, 0x70 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.Program, 0x30 }, 0, 2, 0); // Strings Ensemble
				_output.Send(new byte[] { 0x90, 0x40, 0x70 }, 0, 3, 0);
				_output.Send(new byte[] { 0x80, 0x40, 0x70 }, 0, 3, 0);
				//_output.CloseAsync();
			}
		}

		public void NoteOn(byte note = 60, byte velocity = 127, byte instrument = GeneralMidi.Instruments.AcousticGrandPiano)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { MidiEvent.Program, instrument }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, note, velocity }, 0, 3, 0); // There are constant fields for each MIDI event
			}
		}

		public void NoteOff(byte note = 60, byte velocity = 127)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { MidiEvent.NoteOff, note, velocity }, 0, 3, 0);
			}
		}

		public void AllNotesOff()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_logger.LogInformation("AllNotesOff start");
				_output.Send(new byte[] { MidiEvent.CC, 120, 0 }, 0, 3, 0);
				_logger.LogInformation("AllNotesOff end");
			}
		}

		public void PlayNote(byte note = 60, int milliseconds = 1000, byte velocity = 127, byte instrument = GeneralMidi.Instruments.AcousticGrandPiano)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_logger.LogInformation("PlayNote Start");
				if (note > 0) _output.Send(new byte[] { MidiEvent.Program, instrument }, 0, 2, 0); // There are constant fields for each GM instrument
				if (note > 0) _output.Send(new byte[] { MidiEvent.NoteOn, note, velocity }, 0, 3, 0); // There are constant fields for each MIDI event
				Task.Delay(milliseconds).Wait();
				if (note > 0) _output.Send(new byte[] { MidiEvent.NoteOff, note, velocity }, 0, 3, 0);
				_logger.LogInformation("PlayNote End");
			}
		}

		public void PlayPercussion(PercussionsEnum percussion = PercussionsEnum.BassDrum1, int milliseconds = 1000, byte velocity = 127)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_logger.LogInformation($"PlayPercussion: {percussion}");

				_output.Send(new byte[] { 0xC9, 0x00 }, 0, 2, 0); // Program channel: 0xC9 - 0x00 (Standard Drums Kit = 1 ==> coded 0)
				_output.Send(new byte[] { 0x99 /* 153 */, (byte)percussion, velocity }, 0, 3, 0);  // Play note/percussion on channel 10
				Task.Delay(milliseconds).Wait();
				_output.Send(new byte[] { 0x99 /* 153 */, (byte)percussion, 0 }, 0, 3, 0); // NoteOff --> velocity of zero
			}
		}

		public void PlayNotes(List<NoteModel> notes, int milliseconds, byte velocity = 127, byte instrument = GeneralMidi.Instruments.AcousticGrandPiano)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				foreach (var note in notes)
				{
					PlayNote(note.Note, (int)(note.Duration * milliseconds), velocity, instrument);
				}
			}
		}

		public void PlayDrum(byte note = 60, int milliseconds = 1000, byte velocity = 127, byte instrument = GeneralMidi.Percussions.AcousticBassDrum)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{

				_output.Send(new byte[] { MidiEvent.Program, instrument }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, note, velocity }, 0, 3, 0); // There are constant fields for each MIDI event
				Task.Delay(milliseconds).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, note, velocity }, 0, 3, 0);
			}
		}

		public void PlayFailureSound()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.Trombone }, 0, 2, 0);
				_output.Send(new byte[] { MidiEvent.NoteOn, 40, 127 }, 0, 3, 0);
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 40, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOn, 38, 127 }, 0, 3, 0);
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 38, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOn, 36, 127 }, 0, 3, 0);
				Task.Delay(1000).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 36, 127 }, 0, 3, 0);
			}
		}

		public void PlaySuccessSound()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.Applause }, 0, 2, 0);
				_output.Send(new byte[] { MidiEvent.Pitch, 60, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOn, 60, 127 }, 0, 3, 0);
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOn, 62, 127 }, 0, 3, 0);
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOn, 66, 127 }, 0, 3, 0);
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 60, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 62, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 66, 127 }, 0, 3, 0);
			}
		}

		public void PlayButtonSetSound()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.TelephoneRing }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, 60, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
																				 // _output.Send(new byte[] { MidiEvent.NoteOn, 62, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
																				 // _output.Send(new byte[] { MidiEvent.NoteOn, 66, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 60, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 62, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 66, 127 }, 0, 3, 0);
			}
		}

		public void PlayButtonReleaseSound()
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				_output.Send(new byte[] { 0xC0, GeneralMidi.Instruments.BirdTweet }, 0, 2, 0); // There are constant fields for each GM instrument
				_output.Send(new byte[] { MidiEvent.NoteOn, 50, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
				_output.Send(new byte[] { MidiEvent.NoteOn, 52, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
				_output.Send(new byte[] { MidiEvent.NoteOn, 56, 127 }, 0, 3, 0); // There are constant fields for each MIDI event
				Task.Delay(500).Wait();
				_output.Send(new byte[] { MidiEvent.NoteOff, 50, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 52, 127 }, 0, 3, 0);
				_output.Send(new byte[] { MidiEvent.NoteOff, 56, 127 }, 0, 3, 0);
			}
		}

		public async Task PlayMusic(string filename, int milliseconds = 10000, int offSetMiliseconds = 0)
		{
			_logger.LogInformation("PlayMusic Start");
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{
				try
				{
					if (!string.IsNullOrEmpty(filename))
					{
						var path = Path.Combine(_settings.MidiPath, filename);

						_logger.LogInformation($"♫ ♫ ♫ MidiService PlayMusic: {filename}, ms: {milliseconds}, offSetMs: {offSetMiliseconds}");

						var music = MidiMusic.Read(File.OpenRead(path));
						var player = new MidiPlayer(music, _output);
						player.EventReceived += (MidiEvent e) =>
						{
							if (e.EventType == MidiEvent.Program)
								_logger.LogDebug($"Program changed: Channel:{e.Channel} Instrument:{e.Msb}");
						};

						player.Seek(offSetMiliseconds);
						player.Play();
						player.PlaybackCompletedToEnd += () =>
						{
							_logger.LogInformation("end");
							player = new MidiPlayer(music, _output);
							player.Play();
						};

						Task.Delay(milliseconds).Wait();
						// Console.WriteLine("Type [CR] to stop.");
						// Console.ReadLine();
						player.Stop();
						player.Dispose();
					}

					// Task.Delay(500).Wait();
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Error PlayMusic");
				}
			}
		}

		public async Task PlayMusic(MidiPlayFileModel model)
		{
			if (_output == null)
			{
				_logger.LogError("No midi output found");

			}
			else
			{

				await PlayMusic(model.Filename, model.Milliseconds, model.OffSetMilliseconds);
			}
		}
	}
}