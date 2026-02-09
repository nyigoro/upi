import os
import platform
import shutil
import stat
import urllib.request

DEFAULT_REPO = "nyigoro/UPI"
DEFAULT_VERSION = "latest"


def get_repo() -> str:
    return os.environ.get("UPI_RELEASE_REPO", DEFAULT_REPO)


def get_version() -> str:
    return os.environ.get("UPI_VERSION", DEFAULT_VERSION)


def get_platform() -> str:
    system = platform.system().lower()
    if system.startswith("windows"):
        return "win"
    if system.startswith("darwin"):
        return "macos"
    if system.startswith("linux"):
        return "linux"
    raise RuntimeError(f"Unsupported platform: {system}")


def get_arch() -> str:
    machine = platform.machine().lower()
    if machine in {"x86_64", "amd64"}:
        return "x64"
    if machine in {"aarch64", "arm64"}:
        return "arm64"
    raise RuntimeError(f"Unsupported architecture: {machine}")


def get_asset_name() -> str:
    override = os.environ.get("UPI_ASSET_NAME")
    if override:
        return override

    platform_name = get_platform()
    arch = get_arch()
    ext = ".exe" if platform_name == "win" else ""
    return f"upi-{platform_name}-{arch}{ext}"


def get_bin_dir() -> str:
    root = os.environ.get("UPI_HOME") or os.path.join(os.path.expanduser("~"), ".upi")
    return os.path.join(root, "bin")


def get_binary_path() -> str:
    return os.path.join(get_bin_dir(), get_asset_name())


def get_download_url() -> str:
    repo = get_repo()
    version = get_version()
    asset = get_asset_name()

    if version == "latest":
        return f"https://github.com/{repo}/releases/latest/download/{asset}"

    return f"https://github.com/{repo}/releases/download/{version}/{asset}"


def download(url: str, dest: str) -> None:
    tmp = f"{dest}.tmp"
    with urllib.request.urlopen(url) as response:
        if response.status >= 400:
            raise RuntimeError(f"Download failed: {response.status} {response.reason}")
        with open(tmp, "wb") as handle:
            shutil.copyfileobj(response, handle)
    os.replace(tmp, dest)


def ensure_binary() -> str:
    bin_path = get_binary_path()
    if os.path.exists(bin_path):
        return bin_path

    os.makedirs(get_bin_dir(), exist_ok=True)

    url = get_download_url()
    download(url, bin_path)

    if platform.system().lower().startswith("windows"):
        return bin_path

    mode = os.stat(bin_path).st_mode
    os.chmod(bin_path, mode | stat.S_IXUSR | stat.S_IXGRP | stat.S_IXOTH)
    return bin_path
