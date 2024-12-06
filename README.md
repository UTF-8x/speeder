Speeder is a prometheus exporter for internet speed measurement. Using iPerf3
and/or Ookla Speedtest CLI, Speeder regularly measures your internet (or LAN*)
speed and exports it as prometheus metrics.

\* When runnig in iPerf3 mode, you can either measure against public internet
servers to get your WAN speed or against a local server to measure your LAN
speed.

## Quick Start

To quickly try Speeder, run these commands. (Docker required)

```shell
git clone git@github.com:UTF-8x/speeder.git
cd speeder/Examples
docker compose up -d
```

You can now access Grafana at [http://localhost:3001](http://localhost:3001).

## Deploying Speeder

 - [With Docker](https://github.com/UTF-8x/speeder/wiki/Running-Speeder-in-Docker)
 - [Locally](https://github.com/UTF-8x/speeder/wiki/Installing-Speeder)

## Working on Speeder

1. Clone the repo
1. Run a development server with `dotnet watch` or `dotnet run`
1. Build for prod with `dotnet publish -c Release`

## Configuration

See [The Wiki](https://github.com/UTF-8x/speeder/wiki/Configuration)

## Docker Compose

There's a `compose.yaml` in the `Examples` dir... You can run it
with `docker compose up -d` to get an instance of speeder,
prometheus configured to scrape it and grafana with a premade dashboard.

Due to licensing difficulties with the Speedtest CLI, you will need to first
edit the `compose.yaml` file to accept the Speedtest EULA.

The grafana dashboard will be available on `0.0.0.0:3001`