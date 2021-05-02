using UnityEditor.AssetImporters;
using UnityEngine;

namespace Midi
{
    [ScriptedImporter(1, "mid")]
    public class MidiFileImporter : ScriptedImporter
    {
        public MidiImportSettings ImportSettings = new MidiImportSettings();
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var midiFile = MidiFile.Create(ctx.assetPath, ImportSettings);
            ctx.AddObjectToAsset("main obj", midiFile);
            ctx.SetMainObject(midiFile);
        }
    }
}