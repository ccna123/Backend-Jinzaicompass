# Stage 1: Build code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemBrightSpotBE.csproj", "./"]
RUN dotnet restore "./SystemBrightSpotBE.csproj"
COPY . .
RUN dotnet publish "SystemBrightSpotBE.csproj" -c Release -o /app/publish


# # =========================
# # Stage 2: Runtime (LOCAL)
# # =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5120

ENTRYPOINT ["dotnet", "SystemBrightSpotBE.dll"]

# Stage 2: Runtime (Dùng image của AWS)
# FROM public.ecr.aws/lambda/dotnet:8
# WORKDIR /var/task

# # Cài đặt các thư viện cần thiết để chạy Chrome (headless)
# RUN dnf install -y \
#     atk \
#     cups-libs \
#     gtk3 \
#     libXcomposite \
#     libXcursor \
#     libXdamage \
#     libXext \
#     libXi \
#     libXrandr \
#     libXScrnSaver \
#     libXtst \
#     pango \
#     at-spi2-atk \
#     libdrm \
#     libgbm \
#     libxshmfence \
#     alsa-lib \
#     nss \
#     mesa-libgbm

# # Copy code đã build vào image
# COPY --from=build /app/publish .

# ENTRYPOINT ["dotnet", "SystemBrightSpotBE.dll"]