using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static RimWorld.PsychicRitualRoleDef;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(Precept_Ritual), nameof(Precept_Ritual.ShowRitualBeginWindow))]
    public static class Patch_PreceptRitual_ShowRitualBeginWindow
    {
        public static void Prefix(ref Precept_Ritual __instance, ref Dictionary<string, Pawn> forcedForRole, TargetInfo targetInfo)
        {
            // Unlike most of the patches in this mod, this one is a PREFIX. That's
            // because our entire goal with this one is to front-run the actual game
            // code. Instead of tinkering with the behavior of the method, we're
            // just going to edit one of the arguments to the method call.

            // Check whether this is a Breeding ritual
            if (__instance.def.defName != "Breeding" && __instance.def.defName != "Psybreeding" && __instance.def.defName != "Animabreeding")
            {
                // Oops. We've intervened in the ritual-planning phase for a non-
                // breeding ritual. Do nothing; let the game proceed as normal.
                return;         // Return type is void. We NEVER skip the original.
            }

            // Okay, it's a breeding ritual. Find the relevant pawns.

            // Note: for most breeding-related stuff we'd just lookup the static 
            // variables (LordJob_BreedingRitual.manID). That won't work here, 
            // because we're intervening very early. The static variables aren't
            // populated yet. We need to do our own work.

            // Try to lookup the target bed
            Building_Bed bed = (targetInfo.HasThing ? targetInfo.Thing as Building_Bed : null);
            if (bed == null || bed.GetAssignedPawns() == null)
            {
                // There's something wrong with this bed. Do nothing.
            }
            else if (__instance.def.defName == "Breeding" || __instance.def.defName == "Psybreeding")
            {
                // Setup the argument (note: it will be NULL until we write to it)
                forcedForRole = new Dictionary<string, Pawn>();
                // Assign the pawns
                Pawn man = bed.GetAssignedPawns().Where(p => p.gender == Gender.Male).First();
                Pawn woman = bed.GetAssignedPawns().Where(p => p.gender == Gender.Female).First();

                // Note: because we're intervening very early, normal sanity-checks
                // have not been performed yet. It's possible the people assigned to this
                // bed are a pair of children.
                //
                // We intend to assign these pawns as FORCED participants in the ritual. This
                // will prevent the player from swapping them (good!) but it will also
                // block any future sanity-check logic (bad!). So we must perform filtering
                // right now.

                RitualRole_Man manRole = (RitualRole_Man) __instance.behavior.def.roles.Find(r => r.id == "man");
                RitualRole_Woman womanRole = (RitualRole_Woman)__instance.behavior.def.roles.Find(r => r.id == "woman");
                string assignmentFailureReason;

                if (man != null && manRole != null)
                {
                    if (manRole.AppliesToPawn(man, out assignmentFailureReason, targetInfo))
                    {
                        forcedForRole["man"] = man;
                    }
                    else
                    {
                        Messages.Message("Could not assign man: " + assignmentFailureReason, MessageTypeDefOf.NegativeEvent, true);
                    }
                }
                if (woman != null && womanRole != null)
                {
                    if (womanRole.AppliesToPawn(woman, out assignmentFailureReason, targetInfo))
                    {
                        forcedForRole["woman"] = woman;
                    }
                    else
                    {
                        Messages.Message("Could not assign woman: " + assignmentFailureReason, MessageTypeDefOf.NegativeEvent, true);
                    }
                }
            }
            else if (__instance.def.defName == "Animabreeding")
            {
                // Anima breeding is much more flexible. Players are allowed to rearrange the participants
                // (because "woman impregnates man" is a valid use-case). Therefore we do not setup any
                // Forced roles for this type of ritual.
            }
        }
    }
}
