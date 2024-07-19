#!/bin/bash

#Flags
configuration="Debug"
execute=false
build=false
pull=false
path="$HOME/Projects/SammBot"

#Script name.
script_name="$(basename $0)"

function print_usage()
{
        echo "Usage: ${script_name} [-c BUILD_CONFIG] [-e] [-b] [-r] [-p BOT_PATH] [-h]"
        echo "  -c BUILD_CONFIG : Sets the target config for dotnet. For example, Debug or Release. Default value is Debug."
        echo "  -e              : Add this flag to tell the script to execute the bot."
        echo "  -b              : Add this flag to tell the script to build the bot."
        echo "  -r              : Add this flag to tell the script to pull from the git repository."
        echo "  -p BOT_PATH     : Sets the path where the bot is located. Default is $HOME/Projects/SammBot."
        echo "  -h              : Prints this usage text and exits."
}

while getopts "c:ebrp:h" flag
do
        case "${flag}" in
                c) configuration=${OPTARG};;
                e) execute=true;;
                b) build=true;;
                r) pull=true;;
                p) path=${OPTARG};;
                h) print_usage; exit;;
        esac
done

cd $path

if [ "$pull" = true ] ; then
        echo "Pulling from git..."
        git pull
fi

if [ "$build" = true ] ; then
        echo "Attempting to build Samm-Bot on \"${configuration}\" configuration."
        dotnet build --configuration $configuration
fi

if [ "$execute" = true ] ; then
        cd ./Source/bin/$configuration/net8.0
        ./SammBot.Bot
fi