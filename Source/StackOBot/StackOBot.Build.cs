// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class StackOBot : ModuleRules
{
	public StackOBot(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine" });
	}
}