### IMPORTS ###
import virtualbox
import subprocess, time

### VARIABLES ###
vbox = virtualbox.VirtualBox()
session = virtualbox.Session()
machine = vbox.find_machine("HatchOS")

### MAIN CODE ###
progress = machine.launch_vm_process(session, "gui", [])
time.sleep(10)
subprocess.call("C:\\Program Files\\PuTTY\\plink.exe -serial \\\\.\\pipe\\HatchOS")
progress.wait_for_completion()

        
