# Contributing to FlowT

> **Quick links:** [README](README.md) · [CHANGELOG](CHANGELOG.md) · [Issues](https://github.com/vlasta81/FlowT/issues) · [Pull Requests](https://github.com/vlasta81/FlowT/pulls)

---

## ⚡ Quick Reference — `tasks.ps1`

All common tasks are available through the unified task runner:

```powershell
.\tasks.ps1 status              # Project dashboard
.\tasks.ps1 build               # Build (Release)
.\tasks.ps1 test                # Run all tests
.\tasks.ps1 pack                # Build + NuGet package
.\tasks.ps1 bump Minor          # Bump VersionPrefix Minor
.\tasks.ps1 bump 2.0.0          # Set explicit version
.\tasks.ps1 release 1.2.0       # Create tag + package
.\tasks.ps1 release 1.2.0 beta.1 -SkipTests -Push
.\tasks.ps1 changelog           # Generate CHANGELOG entry from git log
.\tasks.ps1 docs                # Regenerate API documentation
.\tasks.ps1 branch feat/my-feature   # Create feature branch from fresh master
.\tasks.ps1 hotfix 1.2.1             # Create hotfix branch from last release tag
.\tasks.ps1 hotfix 1.2.1 v1.2.0     # Create hotfix branch from specific tag
.\tasks.ps1 pr "Add retry policy"    # Push branch + open draft PR (requires gh)
.\tasks.ps1 pr "Add retry policy" ready  # Open ready-for-review PR
.\tasks.ps1 sync                     # Rebase current branch onto origin/master
.\tasks.ps1 hooks install            # Install git hooks into .git/hooks/
.\tasks.ps1 hooks remove             # Remove git hooks from .git/hooks/
.\tasks.ps1 help                # Full usage reference
```

---

## 🛠️ Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| Visual Studio | 2026 (18.4.1+) | or any editor with .NET SDK |
| .NET SDK | 10.0+ | `dotnet --version` |
| PowerShell | 5.1+ | built into Windows; `pwsh` on Linux/macOS |
| Git | any | must be in `PATH` |
| GitHub CLI | 2.x+ | optional — required for `.\tasks.ps1 pr`; `winget install GitHub.cli` |

No external tools or VS extensions required.

---

## 📁 Project Structure

```
FlowT/
├── tasks.ps1                       # ⭐ Unified task runner (start here)
├── scripts/
│   ├── build.ps1                   # Dev build (called by tasks.ps1)
│   ├── bump-version.ps1            # Version bumper (called by tasks.ps1)
│   └── release.ps1                 # Release script (called by tasks.ps1)
├── Directory.Build.props           # Centralized versioning — single source of truth
├── CHANGELOG.md                    # Version history
├── CONTRIBUTING.md                 # This file
├── src/
│   ├── FlowT/                      # Core library (.NET 10 + .NET Standard 2.0)
│   └── FlowT.Analyzers/            # 26 Roslyn analyzers
├── tests/
│   └── FlowT.Tests/                # 206+ unit tests (xUnit)
├── benchmarks/
│   └── FlowT.Benchmarks/           # BenchmarkDotNet suite
└── samples/
    └── FlowT.SampleApp/            # Complete example application
```

---

## 🔢 Versioning Strategy

FlowT follows **[Semantic Versioning 2.0](https://semver.org/)**:

```
MAJOR.MINOR.PATCH[-SUFFIX.BUILD]

Examples:
1.2.0           – Stable release
1.2.0-dev.42    – Development build (commit #42)
1.2.0-beta.1    – Beta pre-release
1.2.0-rc.1      – Release candidate
2.0.0           – Major with breaking changes
```

### Version Components

| Part | When to bump |
|------|-------------|
| **MAJOR** | Breaking API changes |
| **MINOR** | New backward-compatible features |
| **PATCH** | Bug fixes and small improvements |
| **SUFFIX** | Pre-release identifier (`dev`, `alpha`, `beta`, `rc`) |
| **BUILD** | Git commit count — calculated automatically |

### `Directory.Build.props` — Single Source of Truth

```xml
<PropertyGroup>
  <!-- Only this value is changed manually -->
  <VersionPrefix>1.2.0</VersionPrefix>

  <!-- dev suffix is added automatically by build.ps1 -->
  <VersionSuffix Condition="'$(VersionSuffix)' == ''">dev</VersionSuffix>

  <!-- BuildNumber is calculated from Git commit count -->
  <BuildNumber Condition="'$(BuildNumber)' == ''">0</BuildNumber>
</PropertyGroup>
```

- **Development builds:** `1.2.0-dev.42` (auto-calculated)
- **Stable releases:** `1.2.0` (set via `tasks.ps1 release`)

### Bump Version

```powershell
.\tasks.ps1 bump Patch          # 1.2.0 → 1.2.1
.\tasks.ps1 bump Minor          # 1.2.0 → 1.3.0
.\tasks.ps1 bump Major          # 1.2.0 → 2.0.0
.\tasks.ps1 bump 1.5.0          # set explicit version
.\tasks.ps1 bump Minor -DryRun  # preview without changes
```

### Git Tagging Convention

```
v<version>

v1.2.0         – Stable release
v1.2.0-beta.1  – Pre-release
v2.0.0-rc.1    – Release candidate
```

---

## 💻 Daily Development Workflow

### 1. Open Solution

```
File → Open → Project/Solution → FlowT.sln
```

### 2. Check Project Status

```powershell
.\tasks.ps1 status
```

Shows: current version, branch, last tag, commits since tag, CHANGELOG status, NUGET_API_KEY availability.

### 3. Build

```powershell
.\tasks.ps1 build               # Release (default)
.\tasks.ps1 build -Configuration Debug
```

Or via Visual Studio: **Build → Build Solution** (`Ctrl+Shift+B`)

### 4. Run Tests

```powershell
.\tasks.ps1 test
```

Or via Test Explorer: **Test → Run All Tests** (`Ctrl+R, A`)

### 5. Create Dev Package

```powershell
.\tasks.ps1 pack                # Release build + NuGet package
# Output: artifacts/FlowT.1.2.0-dev.42.nupkg
```

### 6. Commit and Push

Commit message conventions (used by `tasks.ps1 changelog`):

| Prefix | CHANGELOG section | Example |
|--------|------------------|---------|
| `feat:` | Added | `feat: add streaming support` |
| `fix:` | Fixed | `fix: null reference in handler` |
| `perf:` | Performance | `perf: reduce allocations in pipeline` |
| `refactor:` | Changed | `refactor: simplify FlowContext` |
| `test:` | Tests | `test: add specification edge cases` |
| `docs:` | Documentation | `docs: update FlowContext guide` |
| `chore:` | *(skipped)* | `chore: bump analyzer version` |

```powershell
git add .
git commit -m "feat: add new feature"
git push origin master
```

---

## 🚀 Release Process

### 1. Verify Everything is Ready

```powershell
.\tasks.ps1 status      # check branch, uncommitted changes
.\tasks.ps1 test        # all tests must pass
```

### 2. Update CHANGELOG

```powershell
.\tasks.ps1 changelog   # auto-generate entry from git log
```

Then review and edit `CHANGELOG.md` manually to add detail.

### 3. Bump Version (if needed)

```powershell
.\tasks.ps1 bump Minor              # or Patch / Major
.\tasks.ps1 bump Minor -DryRun      # preview first
```

### 4. Create Release

**Stable release:**
```powershell
.\tasks.ps1 release 1.2.0
.\tasks.ps1 release 1.2.0 -Push     # also pushes to GitHub
```

**Pre-release:**
```powershell
.\tasks.ps1 release 1.2.0 beta.1 -SkipTests -Push
```

**Dry run (preview without changes):**
```powershell
.\tasks.ps1 release 1.2.0 -DryRun
```

The release script:
1. ✅ Validates you are on `master` with clean working tree
2. ✅ Checks the tag does not already exist
3. ✅ Builds in Release configuration
4. ✅ Runs all tests (unless `-SkipTests`)
5. ✅ Creates NuGet package in `artifacts/`
6. ✅ Creates an annotated git tag `v<version>`
7. ✅ Shows next steps (push + GitHub Release)

### 5. Publish to NuGet

After pushing the tag, the CI `release.yml` workflow publishes automatically.
Manual push (if needed):

```powershell
$env:NUGET_API_KEY = "your-key"
dotnet nuget push artifacts/FlowT.1.2.0.nupkg --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
```

### 6. Create GitHub Release

After CI completes, create a GitHub Release from the tag and paste the CHANGELOG entry.

---

## 🧪 Adding Tests

All tests live in `tests/FlowT.Tests/`. Maintain 100% coverage for critical paths.

```powershell
.\tasks.ps1 test
```

Guidelines:
- Use xUnit and follow existing naming: `MethodName_Scenario_ExpectedResult`
- Group by feature: `FlowContextTests.cs`, `PluginTests.cs`, `AnalyzerTests/`
- New Roslyn analyzers must have tests in `tests/FlowT.Tests/AnalyzerTests/`

---

## 🛡️ Adding Roslyn Analyzers

1. Create `src/FlowT.Analyzers/FlowTXXXAnalyzer.cs` (next available number)
2. Follow the naming pattern `FlowT002` through `FlowT026`
3. Update `src/FlowT.Analyzers/README.md` with:
   - Rule ID, title, severity
   - Description and Bad ❌ / Good ✅ code examples
   - Rationale
4. Add tests in `tests/FlowT.Tests/AnalyzerTests/`

---

## 📝 Adding Documentation

```powershell
.\tasks.ps1 docs    # regenerate API docs from XML comments
```

- XML comments in source → `docs/api/` via `GenerateDefaultDocumentation.ps1`
- Guides live in `docs/` as Markdown files
- Update `README.md` for significant new features

---

## 📊 Running Benchmarks

```powershell
cd benchmarks/FlowT.Benchmarks
dotnet run -c Release -- --filter '*' --job short
```

Or trigger the [Benchmarks workflow](https://github.com/vlasta81/FlowT/actions/workflows/benchmark.yml) on GitHub Actions.

---

## 🐛 Reporting Issues

Use the [GitHub issue templates](https://github.com/vlasta81/FlowT/issues/new/choose):
- **Bug Report** — reproducible problem
- **Feature Request** — new capability or improvement

---

## 📄 License

MIT — see [LICENSE](LICENSE).
