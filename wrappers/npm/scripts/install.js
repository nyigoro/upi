const fs = require("fs");
const os = require("os");
const path = require("path");
const https = require("https");

const DEFAULT_REPO = "nyigoro/UPI";
const DEFAULT_VERSION = "latest";

function getRepo() {
  return process.env.UPI_RELEASE_REPO || DEFAULT_REPO;
}

function getVersion() {
  return process.env.UPI_VERSION || DEFAULT_VERSION;
}

function getPlatform() {
  switch (process.platform) {
    case "win32":
      return "win";
    case "darwin":
      return "macos";
    case "linux":
      return "linux";
    default:
      throw new Error(`Unsupported platform: ${process.platform}`);
  }
}

function getArch() {
  switch (process.arch) {
    case "x64":
      return "x64";
    case "arm64":
      return "arm64";
    default:
      throw new Error(`Unsupported architecture: ${process.arch}`);
  }
}

function getAssetName() {
  if (process.env.UPI_ASSET_NAME) return process.env.UPI_ASSET_NAME;

  const platform = getPlatform();
  const arch = getArch();
  const ext = process.platform === "win32" ? ".exe" : "";
  return `upi-${platform}-${arch}${ext}`;
}

function getBinDir() {
  const root = process.env.UPI_HOME || path.join(os.homedir(), ".upi");
  return path.join(root, "bin");
}

function getBinaryPath() {
  return path.join(getBinDir(), getAssetName());
}

function getDownloadUrl() {
  const repo = getRepo();
  const version = getVersion();
  const asset = getAssetName();

  if (version === "latest") {
    return `https://github.com/${repo}/releases/latest/download/${asset}`;
  }

  return `https://github.com/${repo}/releases/download/${version}/${asset}`;
}

function download(url, dest) {
  return new Promise((resolve, reject) => {
    const tmp = `${dest}.tmp`;
    const file = fs.createWriteStream(tmp);

    function handleResponse(res) {
      if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
        res.resume();
        return download(res.headers.location, dest).then(resolve).catch(reject);
      }

      if (res.statusCode !== 200) {
        res.resume();
        return reject(new Error(`Download failed: ${res.statusCode} ${res.statusMessage}`));
      }

      res.pipe(file);
      file.on("finish", () => {
        file.close(() => {
          fs.renameSync(tmp, dest);
          resolve();
        });
      });
    }

    https.get(url, handleResponse).on("error", (err) => {
      try {
        fs.unlinkSync(tmp);
      } catch {}
      reject(err);
    });
  });
}

async function ensureBinary() {
  const binPath = getBinaryPath();
  if (fs.existsSync(binPath)) return binPath;

  fs.mkdirSync(getBinDir(), { recursive: true });

  const url = getDownloadUrl();
  await download(url, binPath);

  if (process.platform !== "win32") {
    fs.chmodSync(binPath, 0o755);
  }

  return binPath;
}

if (require.main === module) {
  ensureBinary().catch((err) => {
    console.error("[upi] Failed to download binary:", err.message);
    process.exit(1);
  });
}

module.exports = {
  ensureBinary,
  getBinaryPath
};
