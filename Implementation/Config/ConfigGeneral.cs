﻿using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Babbler.Implementation.Common;
using Il2CppSystem.IO;
using UnityEngine;

namespace Babbler.Implementation.Config;

public static partial class BabblerConfig
{
    private const string ExpectedVersion = "74d012a7e2564fa9badbc23749a9b16c";
    
    public static ConfigEntry<string> Version;

    public static ConfigEntry<bool> Enabled;
    public static ConfigEntry<SpeechMode> Mode;
    public static ConfigEntry<ConfigTemplate> Template;

    public static ConfigEntry<bool> DistortPhoneSpeech;

    public static ConfigEntry<float> ConversationalVolume;
    public static ConfigEntry<float> OverheardVolume;
    public static ConfigEntry<float> PhoneVolume;

    public static ConfigEntry<float> ConversationalEmoteVolume;
    public static ConfigEntry<float> OverheardEmoteVolume;
    public static ConfigEntry<float> PhoneEmoteVolume;
    
    public static ConfigEntry<float> ConversationalShoutMultiplier;
    public static ConfigEntry<float> OverheardShoutMultiplier;
    public static ConfigEntry<float> PhoneShoutMultiplier;

    public static ConfigEntry<float> FemaleThreshold;
    public static ConfigEntry<float> MaleThreshold;
    public static ConfigEntry<float> GenderDiversity;
    
    public static void Initialize(ConfigFile config)
    {
        ProcessOldConfigFile(config);
        
        Enabled = config.Bind("1. General", "Enabled", true,
                              new ConfigDescription("Another method of enabling and disabling Babbler."));

        Mode = config.Bind("1. General", "Mode", SpeechMode.Synthesis,
                           new ConfigDescription("Determines whether citizens will talk with text to speech synthesis, phonetic sounds, or monosyllabic droning."));
        
        Template = config.Bind("1. General", "Template", ConfigTemplate.None,
                           new ConfigDescription("If this anything other than None, the next time you launch the game, settings will be reset and many adjusted to match that template."));
        
        Version = config.Bind("1. General", "Version", string.Empty,
                              new ConfigDescription("Babbler uses this to reset your configuration between major versions. Don't modify it or it will reset your configuration!"));

        DistortPhoneSpeech = config.Bind("1. General", "Distort Phone Speech", true,
                                         new ConfigDescription("When enabled, a band pass is applied to phones to make them sound a little tinnier, like phones."));

        FemaleThreshold = config.Bind("2. Gender", "Female Threshold", 0.49f,
                                      new ConfigDescription("Increase for more female voices, decrease for less, defaults to what the stock game uses for citizens.",
                                                            new AcceptableValueRange<float>(0f, 1f)));
        
        MaleThreshold = config.Bind("2. Gender", "Male Threshold", 0.51f,
                                    new ConfigDescription("Decrease for more male voices, increase for less, defaults to what the stock game uses for citizens.",
                                                          new AcceptableValueRange<float>(0f, 1f)));

        GenderDiversity = config.Bind("2. Gender", "Gender Diversity", 0.05f,
                                      new ConfigDescription("Adds a random element to voice gender selection, increase for more diverse voices.",
                                                            new AcceptableValueRange<float>(0f, 1f)));
        
        ConversationalVolume = config.Bind("3. Volume", "Conversational Volume", 0.7f,
                                           new ConfigDescription("How loud voices will be when you are speaking directly to a person."));
        
        OverheardVolume = config.Bind("3. Volume", "Overheard Volume", 0.3f,
                                      new ConfigDescription("How loud voices that you overhear nearby will be when you are not talking directly to them."));

        PhoneVolume = config.Bind("3. Volume", "Phone Volume", 0.5f,
                                  new ConfigDescription("How loud voices will be when you are talking with a person over the phone."));
        
        ConversationalEmoteVolume = config.Bind("3. Volume", "Conversational Emotes Volume", 0.7f,
                                           new ConfigDescription("How loud emote sound effects will be when you are speaking directly to a person."));
        
        OverheardEmoteVolume = config.Bind("3. Volume", "Overheard Emotes Volume", 0.5f,
                                      new ConfigDescription("How loud emote sound effects that you overhear nearby will be when you are not talking directly to them."));

        PhoneEmoteVolume = config.Bind("3. Volume", "Phone Emotes Volume", 0.6f,
                                  new ConfigDescription("How loud emote sound effects will be when you are talking with a person over the phone."));
        
        ConversationalShoutMultiplier = config.Bind("3. Volume", "Conversational Shout Multiplier", 2.9f,
                                                    new ConfigDescription("When speaking in all caps, how much to multiply the normal conversational volume."));
        
        OverheardShoutMultiplier = config.Bind("3. Volume", "Overheard Shout Multiplier", 6.6f,
                                               new ConfigDescription("When speaking in all caps, how much to multiply the normal overheard volume."));
        
        PhoneShoutMultiplier = config.Bind("3. Volume", "Phone Shout Multiplier", 4f,
                                           new ConfigDescription("When speaking in all caps, how much to multiply the normal phone volume."));
        
        InitializeSynthesis(config);
        InitializePhonetic(config);
        InitializeDroning(config);
        InitializeEmotes(config);

        ProcessUpgrades();
        ProcessTemplates();

        Utilities.Log("BabblerConfig has initialized!", LogLevel.Debug);
    }
    
    private static void ProcessOldConfigFile(ConfigFile newFile)
    {
        string newPath = newFile.ConfigFilePath;
        string oldPath = newPath.Replace("AAAA_", string.Empty);

        try
        {
            if (!File.Exists(oldPath))
            {
                return;
            }
            
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            
            Utilities.Log("Babbler found old config file path, renaming config file!");
            File.Move(oldPath, newPath);
            newFile.Reload();
        }
        catch (Exception e)
        {
            Utilities.Log($"Error processing old config file: {e.Message}", LogLevel.Error);
        }
    }

    private static void ProcessUpgrades()
    {
        if (Version.Value == ExpectedVersion)
        {
            return;
        }

        Utilities.Log("Detected either a new installation or a major upgrade of Babbler, resetting the configuration file!");
        Version.Value = ExpectedVersion;
        Reset();
    }
    
    private static void Reset()
    {
        Enabled.Value = (bool)Enabled.DefaultValue;
        Mode.Value = (SpeechMode)Mode.DefaultValue;
        Template.Value = (ConfigTemplate)Template.DefaultValue;
        DistortPhoneSpeech.Value = (bool)DistortPhoneSpeech.DefaultValue;
        ConversationalVolume.Value = (float)ConversationalVolume.DefaultValue;
        OverheardVolume.Value = (float)OverheardVolume.DefaultValue;
        PhoneVolume.Value = (float)PhoneVolume.DefaultValue;
        ConversationalEmoteVolume.Value = (float)ConversationalEmoteVolume.DefaultValue;
        OverheardEmoteVolume.Value = (float)OverheardEmoteVolume.DefaultValue;
        PhoneEmoteVolume.Value = (float)PhoneEmoteVolume.DefaultValue;
        ConversationalShoutMultiplier.Value = (float)ConversationalShoutMultiplier.DefaultValue;
        OverheardShoutMultiplier.Value = (float)OverheardShoutMultiplier.DefaultValue;
        PhoneShoutMultiplier.Value = (float)PhoneShoutMultiplier.DefaultValue;
        FemaleThreshold.Value = (float)FemaleThreshold.DefaultValue;
        MaleThreshold.Value = (float)MaleThreshold.DefaultValue;
        GenderDiversity.Value = (float)GenderDiversity.DefaultValue;
        
        ResetSynthesis();
        ResetPhonetic();
        ResetDroning();
        ResetEmotes();
    }

    private static void ProcessTemplates()
    {
        if (Template.Value == ConfigTemplate.None)
        {
            return;
        }

        ConfigTemplate templateCache = Template.Value;
        
        Reset();

        const string REALISTIC = "Realistic";
        const string ABSTRACT = "Abstract";
        
        switch (templateCache)
        {
            case ConfigTemplate.TextToSpeech:
                Mode.Value = SpeechMode.Synthesis;
                EmotesTheme.Value = REALISTIC;
                break;
            case ConfigTemplate.AnimalCrossing:
                Mode.Value = SpeechMode.Phonetic;
                EmotesTheme.Value = ABSTRACT;
                break;
            case ConfigTemplate.Undertale:
                Mode.Value = SpeechMode.Droning;
                EmotesTheme.Value = ABSTRACT;
                break;
            case ConfigTemplate.Minions:
                Mode.Value = SpeechMode.Phonetic;
                EmotesTheme.Value = ABSTRACT;
                PhoneticChancePitchVariance.Value = 1f;
                WidenPitchVarianceRange(1.5f, ref PhoneticMinPitchVariance, ref PhoneticMaxPitchVariance);
                break;
            case ConfigTemplate.BanjoKazooie:
                Mode.Value = SpeechMode.Droning;
                EmotesTheme.Value = ABSTRACT;
                DroningChancePitchVariance.Value = 1f;
                WidenPitchVarianceRange(1.5f, ref DroningMinPitchVariance, ref DroningMaxPitchVariance);
                break;
        }
    }
    
    private static void WidenPitchVarianceRange(float multiplier, ref ConfigEntry<float> min, ref ConfigEntry<float> max)
    {
        // We want some templates to sound more melodic than the defaults, so we widen the pitch variance a bit.
        float pitchRange = Mathf.Abs((float)max.DefaultValue - 1f) * multiplier;
        min.Value = 1f - pitchRange;
        max.Value = 1f + pitchRange;
    }
}