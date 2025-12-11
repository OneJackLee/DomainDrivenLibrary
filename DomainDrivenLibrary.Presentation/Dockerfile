FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5240
EXPOSE 7236

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DomainDrivenLibrary.Presentation/DomainDrivenLibrary.Presentation.csproj", "DomainDrivenLibrary.Presentation/"]
COPY ["Sources/DomainDrivenLibrary.Application/DomainDrivenLibrary.Application.csproj", "Sources/DomainDrivenLibrary.Application/"]
COPY ["Sources/DomainDrivenLibrary.Infrastructure/DomainDrivenLibrary.Infrastructure.csproj", "Sources/DomainDrivenLibrary.Infrastructure/"]
COPY ["Sources/DomainDrivenLibrary.Domain/DomainDrivenLibrary.Domain.csproj", "Sources/DomainDrivenLibrary.Domain/"]
COPY ["Common/DomainDrivenLibrary.Application.Abstractions/DomainDrivenLibrary.Application.Abstractions.csproj", "Common/DomainDrivenLibrary.Application.Abstractions/"]
COPY ["Common/DomainDrivenLibrary.Domain.Shared/DomainDrivenLibrary.Domain.Shared.csproj", "Common/DomainDrivenLibrary.Domain.Shared/"]
RUN dotnet restore "DomainDrivenLibrary.Presentation/DomainDrivenLibrary.Presentation.csproj"
COPY . .
WORKDIR "/src/DomainDrivenLibrary.Presentation"
RUN dotnet build "./DomainDrivenLibrary.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DomainDrivenLibrary.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DomainDrivenLibrary.Presentation.dll"]
