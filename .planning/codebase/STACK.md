# Technology Stack

**Analysis Date:** 2026-01-11

## Languages

**Primary:**
- C# 7.3 - Client plugin (`Client/MoreCheckmarks.cs`)
- C# 12 (.NET 9) - Server mod (`Server/MoreCheckmarksBackend.cs`, `Server/MoreCheckmarksRouter.cs`)

**Secondary:**
- None

## Runtime

**Environment:**
- Client: .NET Framework 4.7.1 (BepInEx plugin running in Unity game)
- Server: .NET 9.0 Windows x64 (SPT server mod)

**Package Manager:**
- NuGet (via .csproj PackageReference for server)
- NuGet packages.config (for client)

## Frameworks

**Core:**
- BepInEx 5.x - Unity mod framework for client plugin
- Harmony - Runtime patching library for modifying game behavior
- SPTarkov.Server.Core 4.0.0 - SPT server mod framework

**Testing:**
- None detected - no test framework configured

**Build/Dev:**
- MSBuild - Build system via Visual Studio solution
- Visual Studio - IDE (solution file: `MoreCheckmarks.sln`)

## Key Dependencies

**Critical (Client):**
- 0Harmony - Runtime method patching for game modifications
- Newtonsoft.Json - JSON parsing for server data
- spt-common - SPT client utilities and HTTP communication
- Unity.TextMeshPro - Text rendering for custom UI elements
- BepInEx.Configuration - F12 in-game configuration menu

**Critical (Server):**
- SPTarkov.Common 4.0.0 - Core SPT utilities
- SPTarkov.DI 4.0.0 - Dependency injection framework
- SPTarkov.Server.Core 4.0.0 - Server routing and data access

**Infrastructure:**
- UnityEngine assemblies - Core Unity APIs for UI manipulation

## Configuration

**Environment:**
- Client uses BepInEx ConfigEntry system (F12 menu)
- Server mod metadata defined via `AbstractModMetadata` record
- SPT path configured in .csproj: `<SPTPath>C:\SPT</SPTPath>`

**Build:**
- `Client/MoreCheckmarks.csproj` - Client build configuration
- `Server/MoreCheckmarksBackend.csproj` - Server build configuration
- `MoreCheckmarks.sln` - Visual Studio solution file
- Output goes to `dist/` folder matching SPT structure

## Platform Requirements

**Development:**
- Windows (Windows-specific .NET target)
- Visual Studio with .NET Framework 4.7.1 and .NET 9.0 SDK
- Local SPT installation for reference assemblies (C:\SPT default)

**Production:**
- SPT 4.0.x installation
- BepInEx (included with SPT)
- Distributed as:
  - `BepInEx/plugins/MoreCheckmarks/MoreCheckmarks.dll`
  - `BepInEx/plugins/MoreCheckmarks/MoreCheckmarksAssets`
  - `SPT/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.dll`

---

*Stack analysis: 2026-01-11*
*Update after major dependency changes*
