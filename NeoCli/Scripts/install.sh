#!/bin/bash

# switch to the Scripts directory

BDIR=`dirname ${BASH_SOURCE[0]}`
cd $BDIR

# Install BakedFileService (you need to already have .net 5 installed)

#mkdir /usr/local/lib/T  >/dev/null 2>&1
#rm -rfv  /usr/local/lib/T/BakedFileService  >/dev/null 2>&1

# do it this way so that if you are not in the Scripts directory it will fail
cp -vr ../../NeoFS /T/dotnet/NeoFS/

cp -v neocli.sh /T/scripts/neocli
#chmod +x /usr/local/bin/BakedFileService
