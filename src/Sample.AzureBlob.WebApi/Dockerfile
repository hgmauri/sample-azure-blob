#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Sample.AzureBlob.WebApi/Sample.AzureBlob.WebApi.csproj", "src/Sample.AzureBlob.WebApi/"]
RUN dotnet restore "src/Sample.AzureBlob.WebApi/Sample.AzureBlob.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Sample.AzureBlob.WebApi"
RUN dotnet build "Sample.AzureBlob.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sample.AzureBlob.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sample.AzureBlob.WebApi.dll"]