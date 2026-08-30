#!/usr/bin/env python3
"""Create one Tripo multiview model without persisting or printing the API key."""

from __future__ import annotations

import argparse
import json
import mimetypes
import secrets
import subprocess
import sys
import time
from pathlib import Path
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import urlparse
from urllib.request import Request, urlopen


API_ROOT = "https://api.tripo3d.ai/v2/openapi"
KEYCHAIN_SERVICE = "com.tripo.api.lion-of-seas"
TERMINAL_STATES = {"success", "failed", "cancelled", "banned", "unknown"}


def api_key() -> str:
    result = subprocess.run(
        ["security", "find-generic-password", "-s", KEYCHAIN_SERVICE, "-w"],
        check=True,
        capture_output=True,
        text=True,
    )
    key = result.stdout.strip()
    if not key:
        raise RuntimeError("The Tripo API key is empty in macOS Keychain")
    return key


class ApiClient:
    def __init__(self, key: str) -> None:
        self.authorization = f"Bearer {key}"

    def json_request(
        self,
        method: str,
        url: str,
        *,
        payload: dict[str, Any] | None = None,
        body: bytes | None = None,
        content_type: str | None = None,
        timeout: int = 120,
    ) -> dict[str, Any]:
        headers = {"Authorization": self.authorization, "Accept": "application/json"}
        if payload is not None:
            body = json.dumps(payload).encode("utf-8")
            content_type = "application/json"
        if content_type:
            headers["Content-Type"] = content_type
        request = Request(url, data=body, headers=headers, method=method)
        try:
            with urlopen(request, timeout=timeout) as response:
                raw = response.read()
                status = response.status
        except HTTPError as exc:
            raw = exc.read()
            status = exc.code
        except URLError as exc:
            raise RuntimeError(f"Network error contacting Tripo: {exc.reason}") from exc
        return response_json(raw, status)

    def download(self, url: str, destination: Path) -> None:
        last_error: Exception | None = None
        partial = destination.with_suffix(destination.suffix + ".partial")
        for attempt in range(1, 5):
            request = Request(url, headers={"User-Agent": "LionOfTheSeas-LocalArtPipeline/1.0"})
            try:
                with urlopen(request, timeout=300) as response, partial.open("wb") as handle:
                    while chunk := response.read(1024 * 1024):
                        handle.write(chunk)
                partial.replace(destination)
                return
            except (HTTPError, URLError, OSError) as exc:
                last_error = exc
                if partial.exists():
                    partial.unlink()
                if attempt < 4:
                    print(f"Download interrupted; retrying existing result ({attempt}/4)", flush=True)
                    time.sleep(attempt * 2)
        raise RuntimeError(f"Failed to download Tripo result after retries: {last_error}")


def response_json(raw: bytes, status: int) -> dict[str, Any]:
    try:
        body = json.loads(raw.decode("utf-8"))
    except (ValueError, UnicodeDecodeError) as exc:
        raise RuntimeError(
            f"Tripo returned non-JSON HTTP {status}: {raw[:300]!r}"
        ) from exc
    if not 200 <= status < 300 or body.get("code", 0) != 0:
        raise RuntimeError(
            f"Tripo request failed (HTTP {status}, code={body.get('code')}): "
            f"{body.get('message') or body.get('msg') or 'unknown error'}"
        )
    return body


def upload_image(client: ApiClient, path: Path) -> tuple[str, str]:
    suffix = path.suffix.lower().lstrip(".")
    file_type = "jpg" if suffix in {"jpg", "jpeg"} else suffix
    if file_type not in {"jpg", "png", "webp"}:
        raise ValueError(f"Unsupported image type: {path}")
    mime = mimetypes.guess_type(path.name)[0] or "application/octet-stream"
    boundary = f"----LionOfTheSeas{secrets.token_hex(16)}"
    disposition = f'Content-Disposition: form-data; name="file"; filename="{path.name}"'
    body_bytes = (
        f"--{boundary}\r\n{disposition}\r\nContent-Type: {mime}\r\n\r\n".encode("utf-8")
        + path.read_bytes()
        + f"\r\n--{boundary}--\r\n".encode("utf-8")
    )
    body = client.json_request(
        "POST",
        f"{API_ROOT}/upload/sts",
        body=body_bytes,
        content_type=f"multipart/form-data; boundary={boundary}",
    )
    token = body.get("data", {}).get("image_token")
    if not token:
        raise RuntimeError(f"Upload response did not contain image_token for {path.name}")
    return file_type, token


def find_urls(value: Any, prefix: str = "") -> list[tuple[str, str]]:
    found: list[tuple[str, str]] = []
    if isinstance(value, dict):
        for key, child in value.items():
            child_prefix = f"{prefix}.{key}" if prefix else key
            found.extend(find_urls(child, child_prefix))
    elif isinstance(value, list):
        for index, child in enumerate(value):
            found.extend(find_urls(child, f"{prefix}[{index}]"))
    elif isinstance(value, str) and value.startswith(("https://", "http://")):
        found.append((prefix, value))
    return found


def choose_url(urls: list[tuple[str, str]], terms: tuple[str, ...]) -> str | None:
    for key, url in urls:
        lowered = f"{key} {urlparse(url).path}".lower()
        if all(term in lowered for term in terms):
            return url
    return None


def sanitized_summary(body: dict[str, Any], task_id: str, model_path: Path, preview_path: Path | None) -> dict[str, Any]:
    data = body.get("data", {})
    return {
        "task_id": task_id,
        "status": data.get("status"),
        "progress": data.get("progress"),
        "model_version": "v3.1-20260211",
        "input_order": ["front", "left", "back", "right"],
        "local_model": model_path.name,
        "local_preview": preview_path.name if preview_path else None,
        "note": "Remote signed URLs, image tokens, and credentials intentionally omitted.",
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--front", type=Path)
    parser.add_argument("--left", type=Path)
    parser.add_argument("--back", type=Path)
    parser.add_argument("--right", type=Path)
    parser.add_argument("--task-id", help="Resume an existing task without creating a new one")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--stem", required=True)
    parser.add_argument("--poll-seconds", type=int, default=8)
    parser.add_argument("--timeout-minutes", type=int, default=25)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)

    client = ApiClient(api_key())

    if args.task_id:
        task_id = args.task_id
        print(f"Resuming task: {task_id}", flush=True)
    else:
        inputs = [args.front, args.left, args.back, args.right]
        if any(path is None for path in inputs):
            parser.error("--front, --left, --back, and --right are required unless --task-id is used")
        missing = [str(path) for path in inputs if path is not None and not path.is_file()]
        if missing:
            raise FileNotFoundError("Missing input images: " + ", ".join(missing))
        uploaded: list[dict[str, str]] = []
        for direction, path in zip(("front", "left", "back", "right"), inputs):
            assert path is not None
            print(f"Uploading {direction}: {path.name}", flush=True)
            file_type, token = upload_image(client, path)
            uploaded.append({"type": file_type, "file_token": token})

        payload = {
            "type": "multiview_to_model",
            "model_version": "v3.1-20260211",
            "files": uploaded,
            "texture": True,
            "pbr": True,
            "texture_quality": "standard",
            "geometry_quality": "standard",
            "export_uv": True,
            "render_image": True,
        }
        body = client.json_request("POST", f"{API_ROOT}/task", payload=payload)
        task_id = body.get("data", {}).get("task_id")
        if not task_id:
            raise RuntimeError("Create-task response did not contain task_id")
        print(f"Task created: {task_id}", flush=True)

    deadline = time.monotonic() + args.timeout_minutes * 60
    last_progress: Any = None
    while True:
        body = client.json_request("GET", f"{API_ROOT}/task/{task_id}")
        data = body.get("data", {})
        status = str(data.get("status", "unknown")).lower()
        progress = data.get("progress")
        if progress != last_progress or status in TERMINAL_STATES:
            print(f"Status: {status}; progress: {progress}", flush=True)
            last_progress = progress
        if status in TERMINAL_STATES:
            break
        if time.monotonic() >= deadline:
            raise TimeoutError(f"Task {task_id} did not finish within {args.timeout_minutes} minutes")
        time.sleep(max(2, min(args.poll_seconds, 30)))

    if status != "success":
        raise RuntimeError(f"Task {task_id} ended with status={status}")

    urls = find_urls(body)
    model_url = (
        choose_url(urls, ("pbr", "model"))
        or choose_url(urls, ("pbr", ".glb"))
        or choose_url(urls, ("model", ".glb"))
        or choose_url(urls, (".glb",))
    )
    if not model_url:
        available_keys = ", ".join(key for key, _ in urls)
        raise RuntimeError(f"No downloadable GLB URL found. URL fields: {available_keys}")

    preview_url = (
        choose_url(urls, ("render", ".png"))
        or choose_url(urls, ("render", ".webp"))
        or choose_url(urls, ("render", ".jpg"))
    )
    model_path = args.output / f"{args.stem}_PBR.glb"
    print(f"Downloading model to: {model_path}", flush=True)
    client.download(model_url, model_path)

    preview_path: Path | None = None
    if preview_url:
        extension = Path(urlparse(preview_url).path).suffix.lower()
        if extension not in {".png", ".webp", ".jpg", ".jpeg"}:
            extension = ".webp"
        preview_path = args.output / f"{args.stem}_Preview{extension}"
        print(f"Downloading preview to: {preview_path}", flush=True)
        client.download(preview_url, preview_path)

    summary = sanitized_summary(body, task_id, model_path, preview_path)
    summary_path = args.output / "task_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(f"Summary: {summary_path}", flush=True)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr, flush=True)
        raise SystemExit(1)
