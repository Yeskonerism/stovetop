# STOVETOP - Custom Project Config Builder

**Stovetop** is a lightweight tool for managing per-project runtime configurations and build commands.  

It lets you define how each project is built, run, and configured — all through simple CLI commands.

## Features
- initialise projects and write to a hidden directory
- JSON Formatting
- run and build automatically with pre-defined commands
- use aliases as shorthand for command names (e.g. "r" for "run")
- both pre-run and post-run hooks that execute before and after program runtime
- Create backups when config is overwritten
- built-in command parser
- built-in logger/text-logging
- globally accessible variables set during initialisation (e.g StovetopCore.cs - STOVETOP_CONFIG_PATH)
- Config backup and restore
- .stove/scripts directory for per-project scripting (this also contains auto-generated pre and post-run hooks)