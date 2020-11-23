FROM mcr.microsoft.com/dotnet/sdk:5.0 

WORKDIR /dotnetapp

# copy project.json and restore as distinct layers
COPY netcore.csproj .
RUN dotnet restore

# copy and build everything else
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/netcore.dll"]