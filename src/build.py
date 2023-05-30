### HatchOS Build Script ###
## IMPORTS ##
import subprocess as sp
import sys

## BUILD CODE ##
# Print information
print("[== HatchOS build script ==]\nNote: this script is intended for users who do not want to or cannot use visual studio to compile HatchOS.\n\n\n")
# Call dotnet to build HatchOS
rc = sp.call(["dotnet", "build"])

# If the return code of dotnet isn't 0, there was an error
if(rc != 0): 
    print("ERROR >> Build failed with code {0}!".format(rc))
    input("Press [ENTER] to exit.")
    sys.exit(rc)
else:
    print("Build completed successfully!")
