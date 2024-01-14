﻿using Il2CppInterop.Runtime.Injection;
using BepInEx;
using SOD.Common.BepInEx;
using Babbler.Implementation.Blurbs;
using Babbler.Implementation.Common;
using Babbler.Implementation.Config;
using Babbler.Implementation.Hosts;
using Babbler.Implementation.Synthesis;

namespace Babbler;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class BabblerPlugin : PluginController<BabblerPlugin>
{
    private bool _hasInitializedImmediate;
    private bool _hasInitializedDeferred;
    
    public override void Load()
    {
        base.Load();

        _hasInitializedImmediate = false;
        _hasInitializedDeferred = false;
        
        BabblerConfig.Initialize(Config);
        
        if (!BabblerConfig.Enabled)
        {
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is disabled.");
            return;
        }

        ClassInjector.RegisterTypeInIl2Cpp<SpeakerHost>();       
        Harmony.PatchAll();
        
        InitializeImmediate();
        
        Utilities.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override bool Unload()
    {
        UninitializeImmediate();
        UninitializeDeferred();
        return base.Unload();
    }

    private void InitializeImmediate()
    {
        if (_hasInitializedImmediate)
        {
            return;
        }

        _hasInitializedImmediate = true;
        
        // This must initialize the moment the game starts to "kickstart" Microsoft Speech Synthesis with a silent sound.
        // Without playing this sound immediately, the game will crash. I do not know why.
        if (BabblerConfig.Mode == SpeechMode.Synthesis)
        {
            SynthesisVoiceRegistry.Initialize();
        }
    }

    public void InitializeDeferred()
    {
        if (_hasInitializedDeferred)
        {
            return;
        }

        _hasInitializedDeferred = true;
        
        // Wait for the main menu to load this stuff because FMOD's listener is ready at that time.
        FMODRegistry.Initialize();

        if (BabblerConfig.Mode == SpeechMode.Blurbs)
        {
            BlurbSoundRegistry.Initialize();
        }
    }

    private void UninitializeImmediate()
    {
        if (!_hasInitializedImmediate)
        {
            return;
        }
        
        SpeakerHostPool.CleanupSpeakerHosts();
    }

    private void UninitializeDeferred()
    {
        if (!_hasInitializedDeferred)
        {
            return;
        }
        
        if (BabblerConfig.Mode == SpeechMode.Blurbs)
        {
            BlurbSoundRegistry.Uninitialize();
        }
    }
}