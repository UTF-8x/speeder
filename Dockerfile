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

RUN apt-get update && apt-get install -y iperf3 && apt-get clean

ENV Iperf__ExePath="/usr/bin/iperf3"

EXPOSE 8080

ENTRYPOINT [ "dotnet", "Speeder.dll" ]