#! /bin/bash

find ./Source/1.6 -name "*.cs" | xargs grep -nHP '"[^"]{26,}"' | grep -Ev "SoS.|TranslatorFormattedStringExtensions|.Colorize\(|Log.|Things/|UI/|ThingDef.Named|HarmonyPatch\(|BaseGen.symbolStack.Push" > MagicStrinReport.txt