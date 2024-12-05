#!/usr/bin/env bash

set -e

version_ge() {
	local version1="$1"
	local version2="$2"

	IFS='.' read -ra v1 <<< "$version1"
	IFS='.' read -ra v2 <<< "$version2"

	for i in {0..2}; do
		if [ -z "${v1[i]}" ]; then v1[i]=0; fi
        if [ -z "${v2[i]}" ]; then v2[i]=0; fi
        if [ "${v1[i]}" -gt "${v2[i]}" ]; then return 0; fi
        if [ "${v1[i]}" -lt "${v2[i]}" ]; then return 1; fi
	done

	return 0
}

installed_version=$(dotnet --version 2>/dev/null)

target_config_dir="/etc/speeder"
target_install_dir="/var/lib/speeder"
systemd_unit_path="/etc/systemd/system/speeder.service"

if [ $? -ne 0 ]; then
	echo ".NET Is not installed"
	exit 1
fi

required_dotnet_version="8.0"

if version_ge "$dotnet_version" "8.0"; then
	echo ".NET 8.0 or newer is required"
	exit 1
fi

if ! command -v systemctl &> /dev/null; then
	echo "SystemD is required"
	exit 1
fi

if ! command -v rsync &> /dev/null; then
	echo "rsync is required"
	exit 1
fi

mkdir -p "${target_config_dir}"
mkdir -p "${target_install_dir}"

echo "Checking out the latest release..."

git fetch
git checkout main
git pull origin main

echo "Building a release..."

pushd Speeder || exit 1
rm -rf out
dotnet publish -c Release

pushd bin/Release/net8.0 || exit 1
echo "Installing..."
rsync -av --exclude 'appsettings.json' --exclude 'appsettings.Development.json' "./" "${target_install_dir}/"

popd || exit 1
popd || exit 1

echo "Installing systemd service..."

cat <<EOL > "${systemd_unit_path}"
[Unit]
Description=Speeder
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=1
User=root

ExecStart=/usr/bin/speeder
WorkingDirectory=/var/lib/speeder

StandardOutput=syslog
StandardError=syslog
SyslogIdentifier=speeder

[Install]
WantedBy=multi-user.target
EOL

echo "(re)configuring logging..."

mkdir -p /etc/rsyslog.d || exit 1
touch /etc/rsyslog.d/speeder.conf || exit 1
cat <<EOL > /etc/rsyslog.d/speeder.conf
if $programname == 'speeder' then /var/log/speeder.log
& stop
EOL

mkdir -p /var/log || exit 1
touch /var/log/speeder.log || exit 1
systemstl restart rsyslog || exit 1

echo "Restarting service..."

systemctl daemon-reload || exit 1
systemctl restart speeder || exit 1

echo "Done!"