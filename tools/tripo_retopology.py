#!/usr/bin/env python3
"""Retopologize one existing Tripo task and download the same task result locally."""

from __future__ import annotations

import argparse
import json
import sys
import time
from pathlib import Path
from typing import Any
from urllib.parse import urlparse

from tripo_multiview_to_model import API_ROOT, ApiClient, api_key, find_urls


TERMINAL_STATES = {"success", "failed", "cancelled", "banned", "unknown"}


def select_download(urls: list[tuple[str, str]], extension: str) -> str | None:
    extension = extension.lower()
    for key, url in urls:
        path = urlparse(url).path.lower()
        if path.endswith(extension) or extension in f"{key} {path}".lower():
            return url
    return None


def sanitized_summary(body: dict[str, Any], task_id: str, original_task_id: str, output: Path) -> dict[str, Any]:
    data = body.get("data", {})
    return {
        "task_id": task_id,
        "original_model_task_id": original_task_id,
        "status": data.get("status"),
        "progress": data.get("progress"),
        "operation": "convert_model advanced quad retopology",
        "format": "FBX",
        "face_limit": 35000,
        "local_output": output.name,
        "note": "Remote signed URLs and credentials intentionally omitted.",
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--original-task-id", required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--stem", required=True)
    parser.add_argument("--face-limit", type=int, default=35000)
    parser.add_argument("--poll-seconds", type=int, default=8)
    parser.add_argument("--timeout-minutes", type=int, default=20)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)

    client = ApiClient(api_key())
    payload = {
        "type": "convert_model",
        "format": "FBX",
        "original_model_task_id": args.original_task_id,
        "quad": True,
        "face_limit": args.face_limit,
        "force_symmetry": False,
    }
    body = client.json_request("POST", f"{API_ROOT}/task", payload=payload)
    task_id = body.get("data", {}).get("task_id")
    if not task_id:
        raise RuntimeError("Create-task response did not contain task_id")
    print(f"Retopology task created: {task_id}", flush=True)

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
    result_url = select_download(urls, ".fbx") or select_download(urls, ".zip")
    if not result_url:
        fields = ", ".join(key for key, _ in urls)
        raise RuntimeError(f"No FBX/ZIP result found. URL fields: {fields}")
    extension = ".zip" if urlparse(result_url).path.lower().endswith(".zip") else ".fbx"
    output = args.output / f"{args.stem}{extension}"
    print(f"Downloading retopologized model to: {output}", flush=True)
    client.download(result_url, output)
    summary = sanitized_summary(body, task_id, args.original_task_id, output)
    (args.output / "retopology_summary.json").write_text(
        json.dumps(summary, indent=2), encoding="utf-8"
    )
    print(f"Summary: {args.output / 'retopology_summary.json'}", flush=True)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr, flush=True)
        raise SystemExit(1)
