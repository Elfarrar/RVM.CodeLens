FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RVM.CodeLens.Web/RVM.CodeLens.Web.csproj", "src/RVM.CodeLens.Web/"]
COPY ["src/RVM.CodeLens.Core/RVM.CodeLens.Core.csproj", "src/RVM.CodeLens.Core/"]
RUN dotnet restore "./src/RVM.CodeLens.Web/RVM.CodeLens.Web.csproj"
COPY . .
WORKDIR "/src/src/RVM.CodeLens.Web"
RUN dotnet build "./RVM.CodeLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RVM.CodeLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.CodeLens.Web.dll"]
