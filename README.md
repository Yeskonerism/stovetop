# 🔥 Stovetop

**A modern, cross-platform build automation tool**

Stovetop is a flexible build system that simplifies project configuration and automation across multiple languages and runtimes. Think Make, but with its own readable and concise config format. Cooking, aren't we?

---

# Table of contents

1. [Features](#features)
2. [Quick Start](#quick-start)
   - [Installation](#installation)
   - [Initialize a Project](#initialize-a-project)
   - [Basic Usage](#basic-usage)
3. [Commands](#commands)
4. [Configuration](#configuration)
   - [Example Configuration](#example-configuration)
   - [Supported Runtimes](#supported-runtimes)
5. [Hooks](#hooks)
   - [Hook Types](#hook-types)
   - [Hook Declaration](#hook-declaration)
   - [Example Hook](#example-hook)
6. [Backup Management](#backup-management)
   - [Backup Commands](#backup-commands)
7. [Advanced Usage](#advanced-usage)
   - [Using Variables](#using-variables)
   - [Custom Aliases](#custom-aliases)
8. [Project Structure](#project-structure)
9. [Development](#development)
   - [Building from Source](#building-from-source)
   - [Adding New Commands](#adding-new-commands)
10. [Contributing](#contributing)
11. [Links](#links)
12. [License](#license)

---

## ✨ Features

- 🎯 **Universal Build Tool** - Works with .NET, Python, Node.js, C/C++, Rust, and more
- 📝 **Custom Configuration** - Simple, readable project configs
- 🔧 **Template System** - Quick project initialization with runtime-specific templates
- 🪝 **Hook System** - Run scripts before/after build and run operations
- 🎨 **Custom Aliases** - Create shortcuts for frequently used commands
- 📦 **Variable Substitution** - Use `${VAR}` syntax in commands
- 💾 **Config Backups** - Automatic backup and restore functionality

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
| `config` | `cfg` | View configuration |
| `backup` | `bak`, `bkp` | Manage config backups |

### User Commands

| Command | Aliases | Description |
|---------|---------|-------------|
| `help` | `h` | Show help information |
| `script` | `sc` | Execute a named inline script |

---

## ⚙️ Configuration

Stovetop uses it's own built in format for its configuration file, which is located at `.stove/stovetop.stove`.

### Example Configuration

```js
// Stovetop configuration file
var PROJECT = "my-game"

project("${PROJECT}")
version(0.0.1)

runtime("gcc")

// Variables
var SRC = "src"
var OUT = "bin"
var CFLAGS = "-Wall -Wextra -O2"
var FILES = "src/main.c src/game.c"

// Commands
build_command("${CFLAGS} ${FILES} -I${SRC} -o ${OUT}/${PROJECT}")
executable("${OUT}/${PROJECT}")

// Aliases
alias("clean", "rm -rf ${OUT}")
```

### Supported Runtimes

- **dotnet** - .NET projects
- **python** - Python projects
- **npm** / **node** - Node.js/JavaScript projects
- **gcc** / **g++** / **clang** / **cc** - C/C++ projects
- **rustc** - Rust projects

---

## 🪝 Hooks

Hooks are inline shell scripts that run at specific points in the build/run lifecycle.

### Hook Types

- `pre_build_hook` - Runs before building
- `post_build_hook` - Runs after building
- `pre_run_hook` - Runs before running
- `post_run_hook` - Runs after running

### Hook Declaration

Hooks are created within `.stove/stovetop.stove` using Stovetop's built in declaration functions. 

### Example Hook

```shell
pre_build_hook("
	echo 'Building ${PROJECT} to ${OUT}/${PROJECT}...';

	if [ ! -d '${OUT}' ]; then 
		echo 'Out directory does not exist... creating...';
		mkdir -p ${OUT} && echo 'Directory ${OUT} created.';
	fi
")

post_build_hook("
	if [ -f '${OUT}/${PROJECT}' ]; then 
		sh verify-build-success.sh '${OUT}/${PROJECT}'; 
	else 
		echo 'Stove did not output an executable to ${OUT}/${PROJECT}, you may have errors in your code.'; 
	fi
")
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

```js
var SRC = "src"
var OUT = "bin"
var COMPILER_FLAGS = "-Wall -O3" 

build_command("${COMPILER_FLAGS} ${SRC}/main.c -o ${OUT}/app")
```

### Custom Aliases

```js
alias("test", "dotnet test --verbosity detailed")
alias("watch", "dotnet watch run")
alias("deploy", "dotnet public -c Release")
```

Then use them:
```bash
stove test
stove watch
stove deploy
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
│           ├── handlers/  # Hook, variable, profile handlers
│           └── templates/ # Runtime templates
│
├── Stovetop.ConfigParser/ # Parser for '.stove' format
│   └── src/               # Lexer, Parser, Config model etc.
│
├── Documentation/         # Project documentation
├── Examples/              # Example configurations
└── README.md              # This file (hello!)
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

---

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

---

## 🔗 Links

- **Repository**: https://github.com/Yeskonerism/stovetop
- **Documentation**: See `/Documentation` folder
- **Examples**: See `/Examples` folder

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with 🔥 by someone who got tired of remembering commands.**

