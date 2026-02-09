using System;
using System.IO;
using System.Text;

namespace RollerBall.Helpers;

public static class SoundGenerator
{
    public static void GenerateSounds()
    {
        Directory.CreateDirectory("Assets/Sounds");
        
        GenerateWav("Assets/Sounds/shoot.wav", GenerateShoot());
        GenerateWav("Assets/Sounds/explode.wav", GenerateExplode());
        GenerateWav("Assets/Sounds/pop.wav", GeneratePop());
        GenerateWav("Assets/Sounds/gameover.wav", GenerateGameOver());
    }

    private static void GenerateWav(string filepath, byte[] data)
    {
        if (File.Exists(filepath)) return;

        using (var stream = new FileStream(filepath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            // RIFF header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + data.Length);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            
            // fmt sub-chunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size (16 for PCM)
            writer.Write((short)1); // AudioFormat (1 for PCM)
            writer.Write((short)1); // NumChannels (1 for Mono)
            writer.Write(44100); // SampleRate
            writer.Write(44100 * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
            writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
            writer.Write((short)16); // BitsPerSample
            
            // data sub-chunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(data.Length);
            writer.Write(data);
        }
    }

    private static byte[] GenerateShoot()
    {
        // Frequency sweep 800Hz -> 200Hz, 150ms
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.15);
        byte[] data = new byte[samples * 2];
        
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / samples;
            double freq = 800 - (600 * t);
            short val = (short)(32000 * Math.Sin(2 * Math.PI * freq * i / sampleRate));
            
            // Apply volume envelope (decay)
            val = (short)(val * (1 - t));

            data[i * 2] = (byte)(val & 0xFF);
            data[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        return data;
    }

    private static byte[] GenerateExplode()
    {
        // White noise, 400ms
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.4);
        byte[] data = new byte[samples * 2];
        Random rnd = new Random();
        
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / samples;
            short val = (short)(rnd.Next(-10000, 10000));
            
            // Decay
            val = (short)(val * Math.Pow(1 - t, 2));

            data[i * 2] = (byte)(val & 0xFF);
            data[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        return data;
    }

    private static byte[] GeneratePop()
    {
        // High ping, 50ms
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.05);
        byte[] data = new byte[samples * 2];
        
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / samples;
            double freq = 1200;
            short val = (short)(20000 * Math.Sin(2 * Math.PI * freq * i / sampleRate));
            
            val = (short)(val * (1 - t));

            data[i * 2] = (byte)(val & 0xFF);
            data[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        return data;
    }

    private static byte[] GenerateGameOver()
    {
        // Low slide, 1s
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 1.0);
        byte[] data = new byte[samples * 2];
        
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / samples;
            double freq = 400 - (300 * t);
            short val = (short)(32000 * (t % (1.0/freq) > (0.5/freq) ? 1 : -1)); // Square wave roughly
            
            val = (short)(val * (1 - t));

            data[i * 2] = (byte)(val & 0xFF);
            data[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        return data;
    }
}