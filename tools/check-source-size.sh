#!/usr/bin/env bash

set -euo pipefail

warning_limit=1000
failure_limit=1500
failure_count=0
warning_count=0
checked_count=0
excluded_count=0

repository_root="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "[source-size] Run this check from inside the Git repository." >&2
  exit 2
}

cd "$repository_root"

classify_file() {
  case "$1" in
    Packages/*|Library/*|Temp/*|Obj/*|Build/*|Builds/*)
      printf '%s' "generated"
      ;;
    Assets/Plugins/*|Assets/ThirdParty/*|Assets/Vendor/*|*/Generated/*|*.g.cs|*.generated.cs)
      printf '%s' "vendor/generated"
      ;;
    *)
      printf '%s' "authored"
      ;;
  esac
}

printf '%-11s %-8s %-7s %s\n' "RESULT" "CATEGORY" "LINES" "PATH"

while IFS= read -r -d '' source_file; do
  category="$(classify_file "$source_file")"

  if [[ "$category" != "authored" ]]; then
    excluded_count=$((excluded_count + 1))
    printf '%-11s %-8s %-7s %s\n' "EXCLUDED" "$category" "-" "$source_file"
    continue
  fi

  nonblank_lines="$(awk 'NF { count++ } END { print count + 0 }' "$source_file")"
  checked_count=$((checked_count + 1))

  if (( nonblank_lines >= failure_limit )); then
    failure_count=$((failure_count + 1))
    printf '%-11s %-8s %-7s %s\n' "FAIL" "$category" "$nonblank_lines" "$source_file"
  elif (( nonblank_lines >= warning_limit )); then
    warning_count=$((warning_count + 1))
    printf '%-11s %-8s %-7s %s\n' "WARN" "$category" "$nonblank_lines" "$source_file"
  else
    printf '%-11s %-8s %-7s %s\n' "PASS" "$category" "$nonblank_lines" "$source_file"
  fi
done < <(git ls-files -z -- '*.cs')

printf '\n[source-size] checked=%d warnings=%d failures=%d excluded=%d\n' \
  "$checked_count" "$warning_count" "$failure_count" "$excluded_count"

if (( failure_count > 0 )); then
  echo "[source-size] Authored C# files must stay below ${failure_limit} non-blank lines." >&2
  exit 1
fi

if (( warning_count > 0 )); then
  echo "[source-size] Files at or above ${warning_limit} lines require a recorded split task."
fi

