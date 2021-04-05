#!/bin/bash

# switch to the Scripts directory

BDIR=`dirname ${BASH_SOURCE[0]}`
cd $BDIR

# Install BakedFileService (you need to already have .net 5 installed)

mkdir /usr/local/lib/T  >/dev/null 2>&1
rm -rfv  /usr/local/lib/T/BakedFileService  >/dev/null 2>&1

# do it this way so that if you are not in the Scripts directory it will fail
cp -vr ../../BakedFileService /usr/local/lib/T/BakedFileService/

cp -v bakedfileservice.sh /usr/local/bin/BakedFileService
chmod +x /usr/local/bin/BakedFileService

echo "[Install Service Files]"
cp -v bakedfileservice.service /etc/systemd/system/bakedfileservice.service

echo "[Systemd Daemon Reload]"
systemctl daemon-reload

echo "[Systemd enable service]"
systemctl enable bakedfileservice

echo "[Systemd start service]"
systemctl start bakedfileservice