#!/usr/bin/env python3
"""Generate one Tripo 3D review model from one explicitly approved image."""

from __future__ import annotations

import argparse
import json
import time
from pathlib import Path
from urllib.parse import urlparse

from tripo_multiview_to_model import (
    API_ROOT,
    TERMINAL_STATES,
    ApiClient,
    api_key,
    choose_url,
    find_urls,
    upload_image,
)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--image", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--stem", required=True)
    parser.add_argument("--timeout-minutes", type=int, default=30)
    args = parser.parse_args()
    if not args.image.is_file():
        raise FileNotFoundError(args.image)
    args.output.mkdir(parents=True, exist_ok=True)

    client = ApiClient(api_key())
    print(f"Uploading approved image only: {args.image.name}", flush=True)
    file_type, token = upload_image(client, args.image)
    payload = {
        "type": "image_to_model",
        "model_version": "P1-20260311",
        "file": {"type": file_type, "file_token": token},
        "face_limit": 20000,
        "texture": True,
        "pbr": True,
        "texture_quality": "standard",
        "export_uv": True,
        "render_image": True,
    }
    body = client.json_request("POST", f"{API_ROOT}/task", payload=payload)
    task_id = body.get("data", {}).get("task_id")
    if not task_id:
        raise RuntimeError("Create-task response did not contain task_id")
    print(f"Task created: {task_id}", flush=True)

    deadline = time.monotonic() + args.timeout_minutes * 60
    while True:
        body = client.json_request("GET", f"{API_ROOT}/task/{task_id}")
        data = body.get("data", {})
        status = str(data.get("status", "unknown")).lower()
        print(f"Status: {status}; progress: {data.get('progress')}", flush=True)
        if status in TERMINAL_STATES:
            break
        if time.monotonic() >= deadline:
            raise TimeoutError(f"Task {task_id} exceeded timeout")
        time.sleep(8)
    if status != "success":
        raise RuntimeError(f"Tripo task ended with status={status}")

    urls = find_urls(body)
    model_url = choose_url(urls, ("pbr", ".glb")) or choose_url(urls, ("model", ".glb")) or choose_url(urls, (".glb",))
    if not model_url:
        raise RuntimeError("No GLB URL found in successful response")
    model_path = args.output / f"{args.stem}_PBR.glb"
    client.download(model_url, model_path)

    preview_url = choose_url(urls, ("render", ".png")) or choose_url(urls, ("render", ".webp")) or choose_url(urls, ("render", ".jpg"))
    preview_path = None
    if preview_url:
        extension = Path(urlparse(preview_url).path).suffix.lower()
        if extension not in {".png", ".webp", ".jpg", ".jpeg"}:
            extension = ".png"
        preview_path = args.output / f"{args.stem}_Preview{extension}"
        client.download(preview_url, preview_path)

    summary = {
        "task_id": task_id,
        "status": status,
        "model_version": "P1-20260311",
        "input": args.image.name,
        "local_model": model_path.name,
        "local_preview": preview_path.name if preview_path else None,
        "note": "Credentials, upload token, and remote signed URLs omitted.",
    }
    (args.output / "task_summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(json.dumps(summary, indent=2), flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
