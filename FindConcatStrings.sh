#! /bin/bash

find ./Source/1.6 -name "*.cs" | xargs grep -nHP '\" \+|\"\+|\+\"|\+ \"' | grep -Ev "SoS.|TranslatorFormattedStringExtensions|.Colorize\(|Log.|Things/|UI/|ThingDef.Named|HarmonyPatch\(|BaseGen.symbolStack.Push" > ConcatStrinReport.txt
