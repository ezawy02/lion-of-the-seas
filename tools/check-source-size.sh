#!/usr/bin/env bash

set -euo pipefail

preferred_limit=500
change_limit=1000
absolute_limit=1500
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

  source_lines="$(awk 'END { print NR + 0 }' "$source_file")"
  checked_count=$((checked_count + 1))

  if (( source_lines > absolute_limit )); then
    failure_count=$((failure_count + 1))
    printf '%-11s %-8s %-7s %s\n' "FAIL-ABS" "$category" "$source_lines" "$source_file"
  elif (( source_lines > change_limit )); then
    failure_count=$((failure_count + 1))
    printf '%-11s %-8s %-7s %s\n' "FAIL-SPLIT" "$category" "$source_lines" "$source_file"
  elif (( source_lines > preferred_limit )); then
    warning_count=$((warning_count + 1))
    printf '%-11s %-8s %-7s %s\n' "WARN" "$category" "$source_lines" "$source_file"
  else
    printf '%-11s %-8s %-7s %s\n' "PASS" "$category" "$source_lines" "$source_file"
  fi
done < <(find Assets/_Project ArtSource tools -type f \
  \( -name '*.cs' -o -name '*.shader' -o -name '*.compute' -o -name '*.hlsl' \
     -o -name '*.cginc' -o -name '*.py' -o -name '*.sh' \) -print0)

printf '\n[source-size] checked=%d warnings=%d failures=%d excluded=%d\n' \
  "$checked_count" "$warning_count" "$failure_count" "$excluded_count"

if (( failure_count > 0 )); then
  echo "[source-size] Changed authored files must stay at or below ${change_limit} physical lines." >&2
  echo "[source-size] Legacy files above ${change_limit} are frozen until split; ${absolute_limit} is absolute." >&2
  exit 1
fi

if (( warning_count > 0 )); then
  echo "[source-size] Files above the ${preferred_limit}-line normal target should be kept focused."
fi
