#!/bin/bash

FILE=$1
# No. S before file deemed 'old'
OLDTIME=3
# Get current and file times
CURTIME=$(date +%s)
FILETIME=$(stat $FILE -c %Y)
TIMEDIFF=$(expr $CURTIME - $FILETIME)

# Check if file older
if [ $TIMEDIFF -gt $OLDTIME ]; then
    echo "Executable not built to $1, verify there is no errors in your project."
else
    echo "Executable built to $1 successfully!"
fi