#!/bin/bash

export MONGO_URI='mongodb://feanor,gilraen,radagast/NeoAlexandria?replicaSet=repA'
dotnet /T/dotnet/NeoCli/NeoCli.dll $@
