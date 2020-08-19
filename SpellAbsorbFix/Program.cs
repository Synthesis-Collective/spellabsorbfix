using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace SpellAbsorbFix
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "SpellAbsorbFix.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                }
            );
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            FormKey magInvSummonKey = new FormKey("Skyrim.esm", 0x0a6459);

            foreach (var spell in state.LoadOrder.PriorityOrder.WinningOverrides<ISpellGetter>())
            {
                if (!spell.MenuDisplayObject.FormKey.Equals(magInvSummonKey)) continue;

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
