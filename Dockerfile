# ---------- STAGE 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj & restore dependencies (cache layer)
COPY *.csproj ./
RUN dotnet restore

# copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---------- STAGE 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# copy published output
COPY --from=build /app/publish .

# explicitly tell Kestrel to listen on 8080
ENV ASPNETCORE_URLS=http://+:8080

# expose the default HTTP port
EXPOSE 8080

ENTRYPOINT ["dotnet", "VapeBotApi.dll"]
