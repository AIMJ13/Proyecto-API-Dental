FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DentalHouseWebAPI/DentalHouseWebAPI.csproj", "DentalHouseWebAPI/"]
COPY ["WebAPI.Business/WebAPI.Business.csproj", "WebAPI.Business/"]
COPY ["WebAPI.Core/WebAPI.Core.csproj", "WebAPI.Core/"]
COPY ["WebAPI.DataAccess/WebAPI.DataAccess.csproj", "WebAPI.DataAccess/"]

RUN dotnet restore "DentalHouseWebAPI/DentalHouseWebAPI.csproj"

COPY . .
WORKDIR "/src/DentalHouseWebAPI"
RUN dotnet publish "DentalHouseWebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DentalHouseWebAPI.dll"]
