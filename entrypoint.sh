#!/bin/bash

set -e
run_cmd="dotnet run --urls http://*:80"

exec $run_cmd

