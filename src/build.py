### HatchOS Build Script ###
# Imports
import subprocess as sp

# Build code
rc = sp.call("dotnet build")
if(rc != 0): 
    print("ERROR >> Build failed with code {0}!".format(rc))
    input("Press [ENTER] to exit.")
else:
    print("Build completed successfully!")
