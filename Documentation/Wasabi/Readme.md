# 🔥 Wasabi

**Stovetop's built-in scripting layer**

Wasabi is Stovetops in-house scripting language, used in its own hook files with potential for much more.

## So far, Wasabi only has 3 features:
- Variable creation, setting and subsitution (${VAR})
- Different levels of logging (raw, info, warn, error etc.)
- Shell command calling

## But thats not to say there isn't room for improvement...
- Conditionals
- Filesystem functions
- Looping
<br>...and more!!

### Example file
```Bash
set Project "Minecraft"
set Version 1.21.1

log.info "Starting project ${Project} v${Version}"

set TempText "Temporary text"

shell mkdir temp/
shell echo ${TempText} > temp/temp.txt

shell cat temp/temp.txt