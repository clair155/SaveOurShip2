using RimWorld;
using Verse;

namespace SaveOurShip2
{
    [DefOf]
    public class EP_DefOf
    {
        [MayRequire("vanillaexpanded.gravship")]
        public static ThingDef VGE_PilotCockpit;

        public static ThingDef PilotConsole;

        [MayRequire("vanillaexpanded.gravship")]
        public static ThingDef VGE_PilotBridge;

        [MayRequire("vanillaexpanded.gravship")]
        public static ThingDef VGE_CopilotConsole;

        [MayRequire("vanillaexpanded.gravship")]
        public static DamageDef VGE_AstrofireDamage;

        [MayRequire("vanillaexpanded.gravship")]
        public static ThingDef VGE_Astrofuel;

        static EP_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EP_DefOf));
        }
    }
}
