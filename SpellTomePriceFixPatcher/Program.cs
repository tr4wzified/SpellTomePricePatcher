using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;

namespace SpellTomePriceFixPatcher
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "SpellTomePriceFixPatcher.esp",
                        BlockAutomaticExit = true,
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            float valueMultiplier = 1;
            var jsonPath = Path.Combine(state.ExtraSettingsDataPath, "settings.json");
            JObject json = JObject.Parse(File.ReadAllText(jsonPath));
            if (json != null && json["value_multiplier"] != null)
                valueMultiplier = (float)json["value_multiplier"]!;
            else throw new Exception("value_multiplier not found in settings.json! Please try to use the original json instead.");
            Console.WriteLine("*** DETECTED SETTINGS ***");
            Console.WriteLine("value_multiplier: " + valueMultiplier);
            Console.WriteLine("*************************");

            foreach (var book in state.LoadOrder.PriorityOrder.Book().WinningOverrides())
            {
                if (book.Keywords != null && book.Keywords.Contains(Skyrim.Keyword.VendorItemSpellTome)) {
                    Book bookToModify = book.DeepCopy();
                    bookToModify.Value = (uint)(bookToModify.Value * valueMultiplier);
                    if (book.Value != bookToModify.Value)
                        state.PatchMod.Books.Add(bookToModify);
                }
                else continue;
            }
        }
    }
}
