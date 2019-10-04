# Thin Nuget Proxy

[![Build Status](https://chriswoolum.visualstudio.com/Thin%20Nuget%20Proxy/_apis/build/status/cwoolum.ThinNugetProxy?branchName=master)](https://chriswoolum.visualstudio.com/Thin%20Nuget%20Proxy/_build/latest?definitionId=8&branchName=master)

This is a super thin Nuget Proxy that is used to proxy connections to an Azure Artifacts Nuget Repository.

It is a super stripped down version of [NugetProxy](https://github.com/loic-sharma/NugetProxy)

## Usage

It is meant to be used as a [Service Container](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/service-containers?view=azure-devops&tabs=yaml) in Azure Pipelines so that you can eliminate passing a PAT to your Docker builds allowing you to cache your image layers correctly.

In your Azure Pipleines YAML definition you need to add a service container.

``` yaml
resources:
  containers:
  # This mounts the proxy server as a container. You specify the feed Url for your Azure Artifacts Repo.
  # You must make sure that the build agent has access to the feed. If it's in the same account, it should already be configured that way.
  - container: nuget
    image: cwoolum/thin-nuget-proxy
    ports:
    - 8080:80
    env:
      MIRROR__ACCESSTOKEN: $(System.AccessToken)
      MIRROR__PACKAGESOURCE: "Feed Url for your Azure Artifacts Repo"

pool:
  vmImage: "ubuntu-16.04"

services:
  nuget: nuget

steps:
- bash: |
    echo -e "\e[34mBuilding Service\e[0m"
    docker build -f Dockerfile  -t nugettest:latest .

  displayName: "Build, tag and push image nugettest"
  failOnStderr: true

```

In your Dockerfile, you'll want to make sure that you add the host IP as a source.

``` dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS base
WORKDIR /src

# 172.17.0.1 seems to always be the host IP when used in Azure Pipelines but we don't want to rely on that.
# TODO: Make this work ->> RUN ip -4 route list match 0/0 | cut -d' ' -f3 | awk '{print $1" host.docker.internal"}' >> /etc/hosts

COPY NugetTest.csproj .
RUN dotnet restore NugetTest.csproj -s http://172.17.0.1:8080/v3/index.json -s https://api.nuget.org/v3/index.json
COPY . .

RUN dotnet build NugetTest.csproj -c Release -o /app
```
