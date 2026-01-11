# Technology Stack

**Analysis Date:** 2025-01-11

## Languages

**Primary:**
- C# 7.3 / C# 12 - All application code

**Secondary:**
- None - no build scripts or additional tooling languages

## Runtime

**Environment:**
- .NET Framework 4.7.1 - Client-side BepInEx plugin
- .NET 9.0 (Windows) - Server-side SPT mod

**Build System:**
- MSBuild via Visual Studio 2022 (v17.14)
- Solution file: `MoreCheckmarks.sln`

## Frameworks

**Core:**
- BepInEx - Client-side Unity mod framework for game patching
- SPTarkov.Server.Core - Server-side SPT modding framework
- Harmony - Runtime method patching library for game modifications

**UI:**
- Unity UI - Game's built-in UI system
- TextMeshPro - Advanced text rendering in Unity

**Build/Dev:**
- Visual Studio 2022 - Primary IDE
- NuGet - Package management for server project

## Key Dependencies

**Client (`Client/MoreCheckmarks.csproj`):**
- 0Harmony - Runtime patching for intercepting game methods
- BepInEx - Plugin framework, configuration system (F12 menu)
- Newtonsoft.Json - JSON parsing for server communication
- Assembly-CSharp - Game assemblies (referenced from SPT installation)
- UnityEngine - Core Unity functionality

**Server (`Server/MoreCheckmarksBackend.csproj`):**
- SPTarkov.Common v4.0.0 - Common SPT types and utilities
- SPTarkov.DI v4.0.0 - Dependency injection framework
- SPTarkov.Server.Core v4.0.0 - Server core functionality (routing, database access)

## Configuration

**Environment:**
- `SPTPath` property in .csproj files - Points to local SPT installation (default: `C:\SPT`)
- No .env files - configuration is build-time only

**Runtime Config:**
- BepInEx ConfigurationManager - F12 in-game menu for client settings
- All settings stored via BepInEx Config.Bind() API

**Build:**
- `Client/MoreCheckmarks.csproj` - Client plugin build configuration
- `Server/MoreCheckmarksBackend.csproj` - Server mod build configuration
- `MoreCheckmarks.sln` - Solution combining both projects

## Platform Requirements

**Development:**
- Windows (required for .NET Framework 4.7.1 client, net9.0-windows server)
- Visual Studio 2022 or compatible MSBuild toolchain
- Local SPT installation at configurable path for assembly references

**Production:**
- SPT (Single Player Tarkov) 4.0.x installation
- BepInEx (included with SPT)
- Windows OS (game requirement)

**Output Locations:**
- Client: `dist/BepInEx/plugins/MoreCheckmarks/MoreCheckmarks.dll`
- Server: `dist/SPT/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.dll`

---

*Stack analysis: 2025-01-11*
*Update after major dependency changes*
