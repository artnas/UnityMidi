using UnityEditor.AssetImporters;

namespace Midi
{
    [ScriptedImporter(1, "mid")]
    public class MidiFileImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var midiFile = MidiFile.Create(ctx.assetPath);
            ctx.AddObjectToAsset("main obj", midiFile);
            ctx.SetMainObject(midiFile);
        }
    }
}