# syntax=docker/dockerfile:1
# Builds rpa-mts-inspections per Defra .NET container standards (github.com/DEFRA/defra-docker-dotnetcore).
ARG PARENT_VERSION=2.1.9-dotnet10.0

# Development stage: SDK-based parent image used to restore, test and publish the app.
FROM defradigital/dotnetcore-development:$PARENT_VERSION AS development
ENV TZ="Europe/London"
ARG PARENT_VERSION

# Label the development image with the parent image it extends.
LABEL uk.gov.defra.parent-image=defradigital/dotnetcore-development:${PARENT_VERSION}

# Create directories for app and test files to be copied into.
RUN mkdir -p ./RPA.MTSInspections/ ./RPA.MTSInspections.Tests/

# Install test dependencies.
COPY --chown=dotnet:dotnet RPA.MTSInspections.Tests/*.csproj ./RPA.MTSInspections.Tests/
RUN dotnet restore ./RPA.MTSInspections.Tests/RPA.MTSInspections.Tests.csproj

# Install app dependencies. NUGET_SOURCE is an optional non-secret package source (e.g. an internal feed mirror).
ARG NUGET_SOURCE
COPY --chown=dotnet:dotnet RPA.MTSInspections/*.csproj ./RPA.MTSInspections/
RUN if [ -n "$NUGET_SOURCE" ]; then \
        dotnet restore ./RPA.MTSInspections/RPA.MTSInspections.csproj --source "$NUGET_SOURCE"; \
    else \
        dotnet restore ./RPA.MTSInspections/RPA.MTSInspections.csproj; \
    fi

# Copy in test and app code.
COPY --chown=dotnet:dotnet RPA.MTSInspections.Tests/ ./RPA.MTSInspections.Tests/
COPY --chown=dotnet:dotnet RPA.MTSInspections/ ./RPA.MTSInspections/

# Build the app.
RUN dotnet publish ./RPA.MTSInspections/ -c Release -o /home/dotnet/out

# Expose the app on a defined port, configurable via a build argument.
ARG PORT=8080
ENV PORT=${PORT}
EXPOSE ${PORT}

# Exec form needs a script to interpolate ${PORT}; the script itself execs dotnet so it still
#   receives signals directly rather than a shell.
# Root-owned so the dotnet user can't modify its own startup script; switch back afterwards.
USER root
COPY --chown=root:root dev-entrypoint.sh ./
RUN chmod +x ./dev-entrypoint.sh
USER dotnet
CMD ["./dev-entrypoint.sh"]

# Production stage
FROM defradigital/dotnetcore:$PARENT_VERSION AS production
ARG PARENT_VERSION

# Label the production image with the parent image it extends.
LABEL uk.gov.defra.parent-image=defradigital/dotnetcore-development:${PARENT_VERSION} \
      org.opencontainers.image.title="rpa-mts-inspections" \
      org.opencontainers.image.vendor="DEFRA"

# Expose the app on a defined port, configurable via a build argument.
ARG PORT=8080
# Globalization (not invariant) is required for en-GB date and number formatting.
ENV ASPNETCORE_URLS=http://*:${PORT} \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE ${PORT}

# Switch to root temporarily to install ICU and lock down file permissions.
USER root

# The production parent image ships without ICU, which the en-GB culture needs once invariant mode is off.
RUN apk add --no-cache icu-libs icu-data-full

# Copy the compiled app from the development build stage, assigning root ownership to
#   prevent modification by other users.
COPY --from=development --chown=root:root /home/dotnet/out/ ./

# Remove write permissions, then drop back to the unprivileged user.
RUN chmod -R a-w /home/dotnet
USER dotnet

# Set the default command for production to run the compiled app.
CMD ["dotnet", "RPA.MTSInspections.dll"]

