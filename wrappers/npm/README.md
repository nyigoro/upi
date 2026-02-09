# UPI (npm wrapper)

This package provides a thin CLI wrapper that downloads and runs the native UPI binary.

## Install

```bash
npm install -g @nyigoro/upi
```

## Usage

```bash
upi --help
```

## Environment variables
- `UPI_RELEASE_REPO`: GitHub repo in the form owner/name. Default: `nyigoro/UPI`
- `UPI_VERSION`: GitHub release tag to download. Default: `latest`
- `UPI_ASSET_NAME`: Override the release asset name
- `UPI_HOME`: Override the cache root (default: `~/.upi`)

## Asset naming
The wrapper expects release assets named like:
- `upi-win-x64.exe`
- `upi-linux-x64`
- `upi-macos-x64`
- `upi-macos-arm64`
