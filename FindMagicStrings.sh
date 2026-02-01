#! /bin/bash

# Searches for long magic strings in C#, whic are
# wort rreviewing as possible non-internationalized UI strings.
# Result is filtered and short enough to be worked manually on.
# One work pass on results of this script was alreafy done.

find ./Source/1.6 -name "*.cs" | xargs grep -nHP '"[^"]{13,}"' | grep -Ev "Scribe_|SoS.|TranslatorFormattedStringExtensions|.Colorize\(|Log.|Things/|UI/|ThingDef.Named|HarmonyPatch\(|BaseGen.symbolStack.Push" > MagicStrinReport.txt