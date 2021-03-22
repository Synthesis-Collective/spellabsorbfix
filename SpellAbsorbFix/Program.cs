using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using System.Threading.Tasks;

namespace SpellAbsorbFix
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .SetTypicalOpen(GameRelease.SkyrimSE, "SpellAbsorbFix.esp")
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            foreach (var spell in state.LoadOrder.PriorityOrder.Spell().WinningOverrides())
            {
                if (!spell.MenuDisplayObject.Equals(Skyrim.Static.MAGINVSummon)) continue;

                if (spell.Flags.HasFlag(SpellDataFlag.NoAbsorbOrReflect)) continue;

                foreach (var effect in spell.Effects)
                {
                    if (!effect.BaseEffect.TryResolve(state.LinkCache, out var magicEffect)) continue;

                    if (magicEffect.Archetype.Type != MagicEffectArchetype.TypeEnum.SummonCreature) continue;

                    var modifiedSpell = state.PatchMod.Spells.GetOrAddAsOverride(spell);
                    modifiedSpell.Flags = modifiedSpell.Flags.SetFlag(SpellDataFlag.NoAbsorbOrReflect, true);
                }
            }
        }
    }
}
