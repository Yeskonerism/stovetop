# STOVETOP 🔥
> Custom Project Config Builder

**Stovetop** is a lightweight CLI tool for managing per-project runtime configurations and build commands.
Define how each project is built, run, and configured — all through simple, intuitive commands.

---

## ✨ Features

- 🚀 **Initialize projects** with custom runtime configurations
- ⚡ **Run and build** with pre-defined commands
- 🔗 **Command aliases** for shorthand (e.g., `r` for `run`, `b` for `build`)
- 🪝 **Pre/post hooks** that execute before and after run/build operations
- 💾 **Automatic config backups** when overwriting configurations
- 📝 **Built-in logging** with color-coded output
- 🎯 **Per-project scripting** in `.stove/scripts/` directory
- 🔄 **Backup management** - create, list, and revert to previous configs
- 🛠️ **Custom aliases** - define project-specific shell commands

---

## 🚀 Quick Start

### Installation

> **Note:** Installation scripts for multiple platforms are coming soon! For now, build from source:

```bash
# Clone the repository
git clone https://github.com/Yeskonerism/stovetop.git
cd stovetop

# Build the project
dotnet build

# (Optional) Add to PATH for global access
```

### Basic Usage

```bash
# Initialize a new project with a specific runtime
stove init dotnet

# Or initialize interactively
stove init

# Run your project
stove run

# Build your project
stove build

# View your configuration
stove config view

# Get help
stove help
```

---

## 📖 Commands

### Pipeline Commands

| Command | Aliases | Description | Usage |
|---------|---------|-------------|-------|
| `init` | `i` | Initialize a new project | `stove init [runtime]` |
| `run` | `r` | Run the project | `stove run [--backup <backup-id>]` |
| `build` | `b`, `bld` | Build the project | `stove build [--backup <backup-id>]` |

### Config Commands

| Command | Aliases | Description | Usage |
|---------|---------|-------------|-------|
| `config` | `cfg` | View/edit configuration | `stove config <view\|edit>` |
| `backup` | `bak`, `bkp` | Manage config backups | `stove backup <list\|revert [backup-id]>` |

### User Commands

| Command | Aliases | Description | Usage |
|---------|---------|-------------|-------|
| `help` | `h` | Show help message | `stove help [command]` |

---

## ⚙️ Configuration

When you run `stove init`, Stovetop creates a `.stove/` directory in your project with the following structure:

```
.stove/
├── stovetop.json          # Main configuration file
├── cache/
│   └── backups/           # Automatic config backups
├── profiles/              # Future: build profiles (Debug, Release, etc.)
└── scripts/
    ├── hooks/             # Pre/post run and build hooks
    │   ├── preRunHook.sh
    │   ├── postRunHook.sh
    │   ├── preBuildHook.sh
    │   └── postBuildHook.sh
    └── user/              # Your custom scripts
```

### stovetop.json

Example configuration file:

```json
{
  "project": "MyAwesomeProject",
  "workingDirectory": "/home/user/projects/MyAwesomeProject",
  "runtime": "dotnet",
  "runCommand": "run --",
  "buildCommand": "build",
  "aliases": {
    "test": "dotnet test --verbosity normal",
    "clean": "rm -rf bin obj",
    "deploy": "dotnet publish -c Release"
  }
}
```

### Configuration Options

| Field | Type | Description |
|-------|------|-------------|
| `project` | string | Project name |
| `workingDirectory` | string | Project root directory |
| `runtime` | string | Runtime/compiler to use (e.g., `dotnet`, `python`, `node`) |
| `runCommand` | string | Command arguments to run the project |
| `buildCommand` | string | Command arguments to build the project |
| `aliases` | object | Custom shell commands specific to your project |

---

## 🪝 Hooks & Scripts

Stovetop automatically creates hook scripts in `.stove/scripts/hooks/` when you initialize a project. These hooks execute at specific points in your workflow:

### Available Hooks

- **`preRunHook.sh`** - Runs before `stove run`
- **`postRunHook.sh`** - Runs after `stove run`
- **`preBuildHook.sh`** - Runs before `stove build`
- **`postBuildHook.sh`** - Runs after `stove build`

### Example Hook

```bash
#!/bin/bash
# .stove/scripts/hooks/preRunHook.sh

echo '[HOOK] Checking dependencies...'
dotnet restore
echo '[HOOK] Dependencies ready!'
```

Hooks are automatically made executable on Unix systems.

---

## 🎯 Custom Aliases

Define project-specific commands in your `stovetop.json`:

```json
{
  "aliases": {
    "test": "dotnet test --verbosity normal",
    "clean": "rm -rf bin obj",
    "deploy": "dotnet publish -c Release && scp -r ./bin/Release user@server:/app",
    "db-migrate": "dotnet ef database update"
  }
}
```

Then run them with:

```bash
stove test
stove clean
stove deploy
```

---

## 💾 Backup Management

Stovetop automatically creates backups when you overwrite an existing configuration.

### Backup Commands

```bash
# Create a manual backup
stove backup

# List all backups
stove backup list

# Revert to a specific backup
stove backup revert <backup-id>

# Run/build with a specific backup config (without reverting)
stove run --backup <backup-id>
stove build --backup <backup-id>
```

Backups are stored in `.stove/cache/backups/` with timestamps.

---

## 🔍 Viewing Configuration

```bash
# View entire configuration
stove config view

# View specific fields
stove config view --name
stove config view --runtime
stove config view --run-command
stove config view --build-command
stove config view --aliases
stove config view --working-directory
```

### Flags

| Flag | Shorthand | Description |
|------|-----------|-------------|
| `--name` | `-n` | Show project name |
| `--runtime` | `-r` | Show runtime |
| `--run-command` | `-rc`, `--run` | Show run command |
| `--build-command` | `-bc`, `--build` | Show build command |
| `--working-directory` | `-wd` | Show working directory |
| `--aliases` | `-a` | Show all aliases |

---

## 🛣️ Roadmap

See [Roadmap.md](Roadmap.md) for planned features including:

- 📦 Install scripts for multiple platforms
- ✅ Runtime verification
- 🎨 Interactive console mode
- 📝 Config editing commands
- 🚀 Deploy command with hooks
- 🎭 Build profiles (Debug, Release, etc.)
- 🌍 Global project management

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

---

## 📄 License

[Add your license here]

---

## 👨‍💻 Author

**Oliver Hughes (Yeskonerism)**

---

**Made with 🔥 by a developer who got tired of remembering build commands**