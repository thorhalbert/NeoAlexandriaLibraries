#!/bin/bash

export MONGO_URI='mongodb://feanor,gilraen,radagast/NeoAlexandria?replicaSet=repA'
dotnet /usr/local/lib/T/BakedFileService/BakedFileService.dll $@
