import shutil
import os
import paramiko

rootDir = os.getcwd()
targetDir = os.path.join(rootDir, "Publish")

if os.path.isdir(targetDir):
    shutil.rmtree(targetDir)

print("BEGINNING DOTNET BUILD")
os.system("dotnet publish -c Release -o " + targetDir)

print("BUILD COMPLETE")
print("ZIPPING BUILD")
shutil.make_archive(targetDir, 'zip', targetDir)
print("ZIP COMPLETE")
print("UPLOADING FILE VIA SSH")
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect("43.154.234.161", username="root", password=os.environ["dev_server_key"])
sftp = ssh.open_sftp()
remotePath = "/home/ide-publish.zip"
sftp.put(targetDir+".zip", remotePath)
sftp.close()
print("ZIP UPLOADED")
print("UNZIPPING FILE")
command = "unzip -o /home/ide-publish.zip -d /home/IDE"
stdin,stdout,stderr=ssh.exec_command(command)
outlines=stdout.readlines()
resp=''.join(outlines)
print(resp)
print("UNZIP COMPLETE")
command = "chmod -R 777 /home/IDE/"
stdin,stdout,stderr=ssh.exec_command(command)
outlines=stdout.readlines()
resp=''.join(outlines)
command = "systemctl restart IDE"
stdin,stdout,stderr=ssh.exec_command(command)
outlines=stdout.readlines()
resp=''.join(outlines)
print(resp)
print("COMPLETE!")
ssh.close()