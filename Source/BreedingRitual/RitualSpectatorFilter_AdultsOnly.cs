using RimWorld;
using Verse;

namespace RimWorld
{
    // This class excludes children from joining a breeding ritual.
    // They're obviously not eligible to be participants, but we're filtering
    // them out from serving as spectators, as well.
    //
    // It's mostly intended as a convenience aid to players. I assume that they
    // won't WANT children present, but RimWorld auto-fills a ritual with all
    // possible spectators. So we're just saving them a bunch of clicks.
    //
    // If someone really wants to re-include children (due to mod compatibility
    // issues or whatever) then code patches aren't needed. XML editing will suffice.
    public class RitualSpectatorFilter_AdultsOnly : RitualSpectatorFilter
    {
        public override bool Allowed(Pawn p)
        {
            return p.DevelopmentalStage.Adult();
        }
    }
}
