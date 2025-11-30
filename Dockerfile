# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5200

# Use the SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TuitionApi.csproj", "./"]
RUN dotnet restore "TuitionApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TuitionApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TuitionApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TuitionApi.dll"]
