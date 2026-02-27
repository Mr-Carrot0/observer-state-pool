using Godot;
using System;

public partial class AudioSystem : AudioStreamPlayer
{
    [Export]
    public Node EventHandler; // instead of player, use the Interface for Event Registration.

    [Export] Godot.Collections.Dictionary<EventType, AudioStream> AvailableStreams;

    EventType currentAudioType;
    // AudioStreamPlaybackPolyphonic Playback;

    bool Disabled;

    public override void _Ready()
    {
        // Register on the correct events on "player"
        if (EventHandler is IEventHandler)
        {
            ((IEventHandler)EventHandler).Event += PlayAudio;
        }
        else
        {
            GD.PrintErr("_WARNING: AudioSystem compromised: AudioSystem.Eventhandler is not IEventHandler");
        }
    }
    public void PlayAudio(EventData data)
    {
        if (!Disabled)
        {
            if (data.Type == EventType.DEATH)
            {
                ((IEventHandler)EventHandler).Event -= PlayAudio;
                Disabled = true;

                Stream = AvailableStreams[EventType.DEATH];
                Play();
            }
            else
            {
                if (!Playing) Play();
                AudioStreamPlaybackPolyphonic playback = (AudioStreamPlaybackPolyphonic)GetStreamPlayback();
                playback.PlayStream(AvailableStreams[data.Type]);
            }
        }
    }
}
