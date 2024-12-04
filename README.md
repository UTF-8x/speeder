This is a very quick and dirty Iperf3 to Prometheus exporter.

More feature maybe coming at some point, idk...

## How to Run

You can run it locally...

```shell
dotnet run
```

Or even better, [run it in Docker](https://github.com/users/UTF-8x/packages/container/package/iperf3-exporter)

```shell
docker run -p 8080:8080 ghcr.io/utf-8x/iperf3-exporter:latest
```

Then configure your Prometheus to scrape `http://container-ip:8080/metrics`.
By default the test runs once a minute so set your scrape interval to a minute...

## Configuration

When running locally, edit `appsettings.json`, it's pretty self-explanatory...

In docker, configure with ENVs like so:

```shell
# Set the path to your iperf3 executable
Iperf__ExePath="path_to_iperf_exe"

# Set your target servers. It's an array.
# The number at the end is the index.
# The port can either be a single number (1234) or a range (1234-4567)
# The hostname and port are separated with a # (because I said so)
Iperf__Servers__0="hostname_1#port"
Iperf__Servers__1="hostname_2#port"

# etc... you get the idea...
```

## Prometheus Config

This is very basic... Use it as an inspiration I guess...

```yaml
global:
	scrape_interval: 1m

scrape_configs:
	- job_name: "iperf3"
	  scrape_interval: 1m
	  metrics_path: /metrics
	  static_configs:
		- targets: [ 'container-ip:8080' ]
```

## Docker Compose

There's a `compose.yaml` in the `Examples` dir... You can run it
with `docker compose up -d` to get an instance of this exporter,
prometheus configured to scrape it and grafana.

The grafana dashboard will be available on `0.0.0.0:3001`