using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Text;

public class WavUtility
{
    // const int HEADER_SIZE = 44;
    // private static object bytes;

    // private static FileStream CreateEmpty(string filePath)
    // {
    //     FileStream fileStream = new FileStream(filePath, FileMode.Create);
    //     byte emptyByte = new byte();
    //     for (int i = 0; i < HEADER_SIZE; i++)
    //     {
    //         fileStream.WriteByte(emptyByte);
    //     }
    //     return fileStream;
    // }

    // public static void Save(string filePath, AudioClip clip)
    // {
    //     if (!filePath.ToLower().EndsWith(".wav"))
    //     {
    //         filePath += ".wav";
    //     }

    //     Directory.CreateDirectory(Path.GetDirectoryName(filePath));

    //     using (FileStream fileStream = CreateEmpty(filePath))
    //     {
    //         ConvertAndWrite(fileStream, clip);
    //         WriteHeader(fileStream, clip);
    //     }
    // }

    // public static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    // {
    //     float[] samples = new float[clip.samples];

    //     clip.GetData(samples, 0);

    //     Int16[] intData = new Int16[samples.Length];
    //     Byte[] bytesData = new Byte[samples.Length * 2];

    //     const float rescaleFactor = 32767; // to convert float to Int16
    //     for (int i = 1; i < samples.Length; i++)
    //     {
    //         intData[i] = (short)(samples[i] * rescaleFactor);
    //         Byte[] byteArr = new Byte[2];
    //         byteArr = BitConverter.GetBytes(intData[i]);
    //         byteArr.CopyTo(bytesData, i * 2);

    //     }

    //     fileStream.Write(bytesData, 0, bytesData.Length);
    // }

    // private static void WriteHeader(FileStream fileStream, AudioClip clip)
    // {
    //     int hz = clip.frequency;
    //     int channels = clip.channels;
    //     int samples = clip.samples;

    //     fileStream.Seek(0, SeekOrigin.Begin);

    //     Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
    //     fileStream.Write(riff, 0, 4);

    //     Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
    //     fileStream.Write(chunkSize, 0, 4);

    //     Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
    //     fileStream.Write(wave, 0, 4);

    //     Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
    //     fileStream.Write(fmt, 0, 4);

    //     Byte[] subChunk1 = BitConverter.GetBytes(16);
    //     fileStream.Write(subChunk1, 0, 4);

    //     UInt16 audioFormat = 1; // PCM

    //     Byte[] format = BitConverter.GetBytes(audioFormat);
    //     fileStream.Write(format, 0, 2);

    //     Byte[] numChannels = BitConverter.GetBytes(channels);
    //     fileStream.Write(numChannels, 0, 2);

    //     Byte[] sampleRate = BitConverter.GetBytes(hz);
    //     fileStream.Write(sampleRate, 0, 4);

    //     Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
    //     fileStream.Write(subChunk2, 0, 4);
    // }

    public static byte[] ConvertAudioClipToWav(AudioClip audioClip)
    {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        var wavData = ConvertToWav(samples, audioClip.frequency, audioClip.channels);
        return wavData;
    }

    private static byte[] ConvertToWav(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream())
        using (var writer = new BinaryWriter(memoryStream))
        {
            var sampleCount = samples.Length;
            var dataSize = sampleCount * 2; // 16-bit audio
            var fileSize = 36 + dataSize;

            // RIFF chunk
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // PCM header size
            writer.Write((ushort)1); // PCM format
            writer.Write((ushort)channels); // Number of channels
            writer.Write(frequency); // Sample rate
            writer.Write(frequency * channels * 2); // Byte rate
            writer.Write((ushort)(channels * 2)); // Block align
            writer.Write((ushort)16); // Bits per sample

            // data subchunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            // Audio samples
            foreach (var sample in samples)
            {
                var pcmSample = (short)(Mathf.Clamp(sample, -1f, 1f) * 32767);
                writer.Write(pcmSample);
            }

            return memoryStream.ToArray();
        }
    }
}
