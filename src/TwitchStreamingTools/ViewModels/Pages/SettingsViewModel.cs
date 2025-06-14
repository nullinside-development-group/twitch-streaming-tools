using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;

using ReactiveUI;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your application settings.
/// </summary>
public class SettingsViewModel : PageViewModelBase {
  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The list of possible output devices that exist on the machine.
  /// </summary>
  private ObservableCollection<string> _outputDevices;

  /// <summary>
  ///   The selected output device to send TTS to.
  /// </summary>
  private string? _selectedOutputDevice;

  /// <summary>
  ///   The TTS voice selected to send TTS to.
  /// </summary>
  private string? _selectedTtsVoice;

  /// <summary>
  ///   The list of installed TTS voices on the machine.
  /// </summary>
  private ObservableCollection<string> _ttsVoices;

  /// <summary>
  ///   The volume to play the TTS messages at.
  /// </summary>
  /// <remarks>0 is silent, 100 is full volume.</remarks>
  private uint _ttsVolume;

  /// <summary>
  ///   Initializes a new instance of the <see cref="SettingsViewModel" /> class.
  /// </summary>
  public SettingsViewModel(IConfiguration configuration) {
    _configuration = configuration;

    // Get the list of output devices and set the default to either what we have in the configuration or the system 
    // default whichever is more appropriate.
    var outputDevices = new List<string>();
    for (int i = 0; i < NAudioUtilities.GetTotalOutputDevices(); i++) {
      outputDevices.Add(NAudioUtilities.GetOutputDevice(i).ProductName);
    }

    _outputDevices = new ObservableCollection<string>(outputDevices);
    _selectedOutputDevice = Configuration.GetDefaultAudioDevice();

    // Get the list of TTS voices and set the default to either what we have in the configuration or the system 
    // default whichever is more appropriate.
    using var speech = new SpeechSynthesizer();
    _ttsVoices = new ObservableCollection<string>(speech.GetInstalledVoices().Select(v => v.VoiceInfo.Name));
    _selectedTtsVoice = Configuration.GetDefaultTtsVoice();

    // Get the volume and set the default to either what we have in the configuration or to half-volume. Why half-volume?
    // No one knows. I just figured I'd at least try not to blow anyone's eardrums out. I'm sure this decision will haunt
    // me one day.
    _ttsVolume = Configuration.GetDefaultTtsVolume() ?? 50u;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "SettingsRegular";

  /// <summary>
  ///   The list of output devices available on the machine.
  /// </summary>
  public ObservableCollection<string> OutputDevices {
    get => _outputDevices;
    set => this.RaiseAndSetIfChanged(ref _outputDevices, value);
  }

  /// <summary>
  ///   The selected output device that audio will be sent to.
  /// </summary>
  public string? SelectedOutputDevice {
    get => _selectedOutputDevice;
    set {
      this.RaiseAndSetIfChanged(ref _selectedOutputDevice, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.OutputDevice = value;
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   The list of TTS voices available on the machine.
  /// </summary>
  public ObservableCollection<string> TtsVoices {
    get => _ttsVoices;
    set => this.RaiseAndSetIfChanged(ref _ttsVoices, value);
  }

  /// <summary>
  ///   The selected TTS voice will be used.
  /// </summary>
  public string? SelectedTtsVoice {
    get => _selectedTtsVoice;
    set {
      this.RaiseAndSetIfChanged(ref _selectedTtsVoice, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.TtsVoice = value;
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   The volume to play the TTS messages at.
  /// </summary>
  /// <remarks>0 is silent, 100 is full volume.</remarks>
  public uint TtsVolume {
    get => _ttsVolume;
    set {
      this.RaiseAndSetIfChanged(ref _ttsVolume, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.TtsVolume = value > 0 ? value : 0;
      }

      _configuration.WriteConfiguration();
    }
  }
}