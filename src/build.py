### HatchOS Build Script ###
# Imports
import subprocess as sp
from tkinter import *
from tkinter import messagebox

# Build code
rc = sp.call("dotnet build")
if(rc != 0): 
    print("ERROR >> Build failed with code {0}!".format(rc))
    input("Press [ENTER] to exit.")
else:
    print("Build completed successfully!")
