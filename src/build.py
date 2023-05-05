### HatchOS Build Script ###
## IMPORTS ##
import subprocess as sp

## BUILD CODE ##
# Call dotnet to build HatchOS
rc = sp.call("dotnet build")

# If the return code of dotnet isn't 0, there was an error
if(rc != 0): 
    print("ERROR >> Build failed with code {0}!".format(rc))
    input("Press [ENTER] to exit.")
else:
    print("Build completed successfully!")
