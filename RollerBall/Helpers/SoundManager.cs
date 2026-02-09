using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace RollerBall.Helpers;

public class SoundManager : IDisposable
{
    private AudioPlaybackEngine? _audioEngine;
    private readonly Dictionary<string, CachedSound> _sounds = new();
    private bool _isSoundEnabled = true;

    public bool IsSoundEnabled 
    { 
        get => _isSoundEnabled; 
        set => _isSoundEnabled = value; 
    }

    public SoundManager()
    {
        try
        {
            _audioEngine = new AudioPlaybackEngine();
            LoadSounds();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio initialization failed: {ex.Message}");
            _isSoundEnabled = false;
        }
    }

    private void LoadSounds()
    {
        if (!Directory.Exists("Assets/Sounds")) return;

        LoadSound("shoot", "Assets/Sounds/shoot.wav");
        LoadSound("explode", "Assets/Sounds/explode.wav");
        LoadSound("pop", "Assets/Sounds/pop.wav");
        LoadSound("gameover", "Assets/Sounds/gameover.wav");
    }

    private void LoadSound(string key, string path)
    {
        if (File.Exists(path))
        {
            try
            {
                var cachedSound = new CachedSound(path);
                if (cachedSound.AudioData != null && cachedSound.AudioData.Length > 0)
                {
                    _sounds[key] = cachedSound;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load sound {path}: {ex.Message}");
            }
        }
    }

    public void PlayShoot() => Play("shoot");
    public void PlayExplode() => Play("explode");
    public void PlayPop() => Play("pop");
    public void PlayGameOver() => Play("gameover");

    private void Play(string key)
    {
        if (!_isSoundEnabled || _audioEngine == null || !_sounds.ContainsKey(key)) return;
        try
        {
            _audioEngine.PlaySound(_sounds[key]);
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error playing sound {key}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _audioEngine?.Dispose();
    }
}

public class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }

    public CachedSound(string audioFileName)
    {
        using (var audioFileReader = new AudioFileReader(audioFileName))
        {
            WaveFormat = audioFileReader.WaveFormat;
            var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
            var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
            AudioData = wholeFile.ToArray();
        }
    }
}

public class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound _cachedSound;
    private long _position;

    public CachedSoundSampleProvider(CachedSound cachedSound)
    {
        _cachedSound = cachedSound;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = _cachedSound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
        _position += samplesToCopy;
        return (int)samplesToCopy;
    }

    public WaveFormat WaveFormat => _cachedSound.WaveFormat;
}

public class AudioPlaybackEngine : IDisposable
{
    private readonly IWavePlayer _outputDevice;
    private readonly MixingSampleProvider _mixer;

    public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
        _outputDevice = new WaveOutEvent();
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
        _mixer.ReadFully = true;
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    public void PlaySound(CachedSound sound)
    {
        ISampleProvider input = new CachedSoundSampleProvider(sound);
        if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
        {
            input = new MonoToStereoSampleProvider(input);
        }
        _mixer.AddMixerInput(input);
    }

    public void Dispose()
    {
        _outputDevice.Dispose();
    }
}
