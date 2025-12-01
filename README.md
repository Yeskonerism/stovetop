# 🔥 Stovetop

**A modern, cross-platform build automation tool with integrated scripting support.**

Stovetop is a flexible build system that simplifies project configuration and automation across multiple languages and runtimes. Think Make, but with YAML configs and a built-in scripting language.

---

## ✨ Features

- 🎯 **Universal Build Tool** - Works with .NET, Python, Node.js, C/C++, Rust, and more
- 📝 **YAML Configuration** - Simple, readable project configs
- 🔧 **Template System** - Quick project initialization with runtime-specific templates
- 🪝 **Hook System** - Run scripts before/after build and run operations
- 🎨 **Custom Aliases** - Create shortcuts for frequently used commands
- 📦 **Variable Substitution** - Use `${VAR}` syntax in commands
- 💾 **Config Backups** - Automatic backup and restore functionality
- 🌊 **Wasabi Scripting** - Built-in scripting language for automation

---

## 🚀 Quick Start

### Installation

```bash
# Clone and build
git clone https://github.com/Yeskonerism/stovetop.git
cd stovetop
dotnet build

# Add to PATH (optional)
export PATH="$PATH:/path/to/stovetop/bin"
```

### Initialize a Project

```bash
# Interactive setup
stove init

# Or specify runtime directly
stove init dotnet
stove init python
stove init gcc
stove init npm
```

### Basic Usage

```bash
# Build your project
stove build

# Run your project
stove run

# View configuration
stove config

# Get help
stove help
```

---

## 📖 Commands

### Pipeline Commands

| Command | Aliases | Description |
|---------|---------|-------------|
| `init` | `i` | Initialize a new Stovetop project |
| `build` | `b`, `bld` | Build the project |
| `run` | `r` | Run the project |

### Configuration Commands

| Command | Aliases | Description |
|---------|---------|-------------|
| `config` | `cfg` | View and edit configuration |
| `backup` | `bak`, `bkp` | Manage config backups |

### User Commands

| Command | Aliases | Description |
|---------|---------|-------------|
| `help` | `h` | Show help information |
| `script` | `sc` | Execute a Wasabi script |

---

## ⚙️ Configuration

Stovetop uses a YAML configuration file located at `.stove/stovetop.config.yaml`.

### Example Configuration

```yaml
project: MyProject
version: 1.0.0

stovetop:
  # Variables (use ${VAR} in commands)
  variables:
    SRC: src
    OUT: bin
    CFLAGS: -Wall -Wextra -O2

  # Runtime configuration
  runtime:
    type: dotnet
    version: net9.0

  # Commands
  commands:
    build: build
    run: run --
    executable: ${OUT}/app
    test: test
    clean: clean
    deploy: publish -c Release

  # Custom aliases
  aliases:
    t: dotnet test
    watch: dotnet watch run

  # Hooks (optional)
  hooks: null

  # Profiles (optional)
  profiles: null
```

### Supported Runtimes

- **dotnet** - .NET projects
- **python** - Python projects
- **npm** / **node** - Node.js/JavaScript projects
- **gcc** / **g++** / **clang** / **cc** - C/C++ projects
- **rustc** - Rust projects

---

## 🪝 Hooks

Hooks are Wasabi scripts that run at specific points in the build/run lifecycle.

### Hook Types

- `pre-build.wasabi` - Runs before building
- `post-build.wasabi` - Runs after building
- `pre-run.wasabi` - Runs before running
- `post-run.wasabi` - Runs after running

### Hook Location

Hooks are stored in `.stove/scripts/hooks/`

### Example Hook

```wasabi
# .stove/scripts/hooks/pre-build.wasabi
log.info "Starting build for ${PROJECT} v${VERSION}"
shell "git rev-parse --short HEAD > .build-commit"
log.success "Pre-build checks complete"
```

---

## 🌊 Wasabi Scripting

Wasabi is Stovetop's built-in scripting language for automation tasks.

### Running Scripts

```bash
# Run a Wasabi script
stove script build-and-deploy.wasabi
stove sc my-script.wasabi
```

### Available Commands

#### Logging
```wasabi
log.info "Information message"
log.warn "Warning message"
log.error "Error message"
log.debug "Debug message"
log.success "Success message"
```

#### Shell Commands
```wasabi
shell "ls -la"
shell "git status"
shell "dotnet test"
```

### Variables

Wasabi scripts have access to all Stovetop variables:

- `${PROJECT}` - Project name
- `${VERSION}` - Project version
- `${RUNTIME}` - Runtime type
- `${RUNTIME_VERSION}` - Runtime version
- `${CWD}` - Current working directory
- `${SCRIPT_DIR}` - Script directory
- All custom variables from `stovetop.config.yaml`

### Example Script

```wasabi
# deploy.wasabi
log.info "Deploying ${PROJECT} v${VERSION}"

log.info "Running tests..."
shell "dotnet test"

log.info "Building release..."
shell "dotnet publish -c Release -o ${OUT}"

log.info "Deploying to server..."
shell "scp -r ${OUT}/* user@server:/apps/${PROJECT}/"

log.success "Deployment complete!"
```

---

## 💾 Backup Management

Stovetop automatically creates backups when you modify configurations.

### Backup Commands

```bash
# Create a backup
stove backup

# List all backups
stove backup list
stove backup list --info

# Revert to a backup
stove backup revert <backup-id>
stove backup revert latest

# Clean old backups (coming soon)
stove backup clean
```

---

## 🎯 Advanced Usage

### Using Variables

```yaml
variables:
  SRC: src
  OUT: bin/Release
  COMPILER_FLAGS: -O3 -Wall

commands:
  build: ${COMPILER_FLAGS} ${SRC}/*.c -o ${OUT}/app
```

### Custom Aliases

```yaml
aliases:
  test: dotnet test --verbosity detailed
  watch: dotnet watch run
  deploy: dotnet publish -c Release
```

Then use them:
```bash
stove test
stove watch
stove deploy
```

### Backup Flags

Run commands with automatic backup:
```bash
stove run --backup
stove build --backup my-backup-name
```

---

## 📁 Project Structure

```
Stovetop/
├── Stovetop/              # Main CLI application
│   └── src/
│       ├── commands/      # Command implementations
│       │   ├── config/    # Config-related commands
│       │   ├── pipeline/  # Build/run commands
│       │   └── user/      # User-facing commands
│       └── stovetop/      # Core functionality
│           ├── config/    # Configuration system
│           ├── handlers/  # Hook, variable, profile handlers
│           └── templates/ # Runtime templates
│
├── Wasabi/                # Wasabi scripting language
│   └── src/
│       ├── commands/      # Wasabi commands (log, shell, etc.)
│       ├── core/          # Interpreter core
│       ├── runtime/       # Runtime services
│       └── utils/         # Utilities
│
├── Documentation/         # Project documentation
├── Examples/              # Example configurations
└── README.md             # This file
```

---

## 🛠️ Development

### Building from Source

```bash
# Build the solution
dotnet build Stovetop.sln

# Run Stovetop
dotnet run --project Stovetop/Stovetop.csproj -- <command>
```

### Adding New Commands

1. Create a new command class in `Stovetop/src/commands/`
2. Register it in `CommandRegistry.cs`
3. Implement the `Run()` method

### Adding Wasabi Commands

1. Create a new command class implementing `IWasabiCommand`
2. Register it in `WasabiExecutor.cs`

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

---

## 🔗 Links

- **Repository**: https://github.com/Yeskonerism/stovetop
- **Documentation**: See `/Documentation` folder
- **Examples**: See `/Examples` folder

---

**Built with 🔥 by someone who got tired of remembering commands.**

