"""Verify a CI package and produce the exact files attached to a GitHub release."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import zipfile


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def prepare(package, output, commit):
    package = package.resolve()
    output = output.resolve()
    manifest = package / "SHA256SUMS.txt"
    for line in manifest.read_text(encoding="utf-8-sig").splitlines():
        if not line.strip():
            continue
        expected, name = line.split("  ", 1)
        path = (package / name).resolve()
        if not path.is_relative_to(package) or not path.is_file():
            raise ValueError("Invalid package checksum path: " + name)
        if sha256(path) != expected:
            raise ValueError("Package checksum mismatch: " + name)
    for name in ["SCP153.dll", "Audio/HAPS.ogg", "Schematics/SCP153/SCP153.json", "config/default.yml"]:
        if not (package / name).is_file():
            raise ValueError("Missing release asset: " + name)
    if any(p.name == "REFERENCES" for p in package.rglob("*")):
        raise ValueError("Server reference libraries must not be distributed")

    info = {
        "plugin": "SCP153",
        "version": "2.2.0",
        "commit": commit,
        "target": "net48-x64",
        "validation": {
            "build": "passed",
            "logic_regression_cases": 34,
            "runtime_server_client": "not performed",
        },
        "build_references": {"exiled": "9.12.6", "scp_sl": "14.2.5"},
        "hume_shield": {"base": 400, "per_bite": 50, "limit": 2000},
    }
    (package / "BUILDINFO.json").write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
    lines = [sha256(p) + "  " + p.relative_to(package).as_posix()
             for p in sorted(package.rglob("*")) if p.is_file() and p != manifest]
    manifest.write_text("\n".join(lines) + "\n", encoding="utf-8")

    output.mkdir(parents=True, exist_ok=True)
    shutil.copy2(package / "SCP153.dll", output / "SCP153.dll")
    archive = output / "SCP153-2.2.0.zip"
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as handle:
        for path in sorted(package.rglob("*")):
            if path.is_file():
                handle.write(path, path.relative_to(package).as_posix())
    with zipfile.ZipFile(archive) as handle:
        if handle.testzip() is not None:
            raise ValueError("Invalid ZIP archive")
    (output / "SHA256SUMS.txt").write_text(
        "\n".join(sha256(p) + "  " + p.name for p in [output / "SCP153.dll", archive]) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--package", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--commit", required=True)
    args = parser.parse_args()
    prepare(args.package, args.output, args.commit)
