#!/usr/bin/env bash

if [ -z "${SPEEDER_OOKLA_EULA}" ]; then
	echo "You must accept the Ookla Speedtest EULA."
	echo "See https://www.speedtest.net/about/eula"
	echo "Set SPEEDER_OOKLA_EULA to 'ACCEPT' to accept the EULA"
	exit 1
fi

if [ "${SPEEDER_OOKLA_EULA}" != "ACCEPT" ]; then
	echo "Please set SPEEDER_OOKLA_EULA to 'ACCEPT' to accept the EULA."
	exit 1
fi

echo "Speedtest EULA accepted, launching Speeder..."
dotnet Speeder.dll