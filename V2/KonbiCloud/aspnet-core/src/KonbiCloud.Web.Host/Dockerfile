FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS runtime

WORKDIR /app
COPY ./bin/Release/netcoreapp2.1/publish/ ./

#expose port for app
ENV ASPNETCORE_URLS=http://*:22743

EXPOSE 22743

# this is for issue that can not update image through this api http://domainname/file/upload?thumbWidth=120&thumbHeight=120
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "KonbiCloud.Web.Host.dll"]