FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /source

COPY *.sln .
COPY Speeder/*.csproj ./Speeder/
RUN dotnet restore

COPY Speeder/. ./Speeder/
WORKDIR /source/Speeder
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=builder /app ./

# Install iPerf3 and Speedtest CLI
RUN apt-get update && apt-get install -y iperf3 curl
RUN curl -s https://packagecloud.io/install/repositories/ookla/speedtest-cli/script.deb.sh | bash && apt-get update && apt-get install speedtest
RUN apt-get clean

RUN mkdir -p /root/.config/ookla

COPY speedtest-cli.json /root/.config/ookla/speedtest-cli.json
COPY docker-entrypoint.sh /app/docker-entrypoint.sh

ENV Iperf__ExePath="/usr/bin/iperf3"
ENV Speedtest__ExePath=""

EXPOSE 8080

ENTRYPOINT [ "bash", "docker-entrypoint.sh" ]