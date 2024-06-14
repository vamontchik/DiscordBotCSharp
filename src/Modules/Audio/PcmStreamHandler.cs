﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace DiscordBot.Modules.Audio;

internal interface IPcmStreamHandler
{
    Task<AudioOutStream?> CreatePcmStreamAsync(
        string url,
        IGuild guild,
        IAudioClient client);

    Task FlushPcmStreamAsync(
        IGuild guild,
        string url,
        Stream pcmStream);
}

internal sealed class PcmStreamHandler(IAudioStore audioStore, IAudioLogger audioLogger) : IPcmStreamHandler
{
    public Task<AudioOutStream?> CreatePcmStreamAsync(
        string url,
        IGuild guild,
        IAudioClient client)
    {
        AudioOutStream pcmStream;
        try
        {
            audioLogger.LogWithGuildInfo(guild, $"Creating pcm stream of {url} in {guild.Name}");
            pcmStream = client.CreatePCMStream(AudioApplication.Mixed);
        }
        catch (Exception e)
        {
            audioLogger.LogExceptionWithGuildInfo(guild, e);
            return Task.FromResult<AudioOutStream?>(null);
        }

        return audioStore.AddPcmStreamForGuild(guild, pcmStream)
            ? Task.FromResult<AudioOutStream?>(pcmStream)
            : Task.FromResult<AudioOutStream?>(null);
    }

    public async Task FlushPcmStreamAsync(IGuild guild, string url, Stream pcmStream)
    {
        try
        {
            audioLogger.LogWithGuildInfo(guild, "Flushing pcm stream");
            await pcmStream.FlushAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            audioLogger.LogExceptionWithGuildInfo(guild, e);
        }
    }
}