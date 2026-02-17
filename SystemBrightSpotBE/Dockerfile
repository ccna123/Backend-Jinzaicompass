FROM cgr.dev/chainguard/dotnet-sdk:latest AS build
WORKDIR /src

COPY ["SystemBrightSpotBE.csproj", "./"]
RUN dotnet restore "./SystemBrightSpotBE.csproj"

COPY --chown=nonroot:nonroot . .

RUN dotnet publish "SystemBrightSpotBE.csproj" -c Release -o ./publish \
    -p:PublishReadyToRun=true


FROM public.ecr.aws/lambda/dotnet:8.2026.02.11.05
WORKDIR /var/task
COPY --from=build /src/publish .
ENTRYPOINT ["dotnet", "SystemBrightSpotBE.dll"]
