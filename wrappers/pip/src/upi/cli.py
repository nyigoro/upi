import subprocess
import sys

from .installer import ensure_binary, get_binary_path


def main() -> None:
    try:
        ensure_binary()
    except Exception as exc:
        print(f"[upi] Failed to initialize: {exc}", file=sys.stderr)
        raise SystemExit(1) from exc

    bin_path = get_binary_path()
    result = subprocess.run([bin_path, *sys.argv[1:]])
    raise SystemExit(result.returncode)
