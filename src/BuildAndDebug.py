### HatchOS Build Script ###
## IMPORTS ##
import subprocess as sp

## BUILD CODE ##
# Call build and debug scripts
print("[== STARTING BUILD ==]")
rc = sp.call(["python", "Build.py"])

if(rc == 0):
    print("[== STARTING DEBUG ==]")
    sp.call(["python", "VBoxDebug.py"])
