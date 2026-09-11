#!/usr/bin/env bash
set -euo pipefail

root="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "[level01] Run this check from inside the Git repository." >&2
  exit 2
}
cd "$root"

scene="Assets/_Project/Scenes/Level_01_HundredSails.unity"
trial="Assets/_Project/Scenes/Trials/Level_01_Playable_Trial.unity"
copy="Assets/_Project/Scripts/UI/Levels/Level01TrialLocalization.cs"
fail=0

require_text() {
  local file="$1"
  local needle="$2"
  if ! grep -F -q -- "$needle" "$file"; then
    echo "FAIL  missing '$needle' in $file" >&2
    fail=1
  fi
}

require_file() {
  if [[ ! -f "$1" ]]; then
    echo "FAIL  missing file $1" >&2
    fail=1
  fi
}

require_guid_in_scene() {
  local asset="$1"
  local meta="${asset}.meta"
  require_file "$asset"
  require_file "$meta"
  local guid
  guid="$(awk '/^guid:/{print $2; exit}' "$meta")"
  if [[ -z "$guid" ]]; then
    echo "FAIL  no guid in $meta" >&2
    fail=1
    return
  fi
  if ! grep -F -q -- "$guid" "$scene"; then
    echo "FAIL  scene does not instance $asset ($guid)" >&2
    fail=1
  fi
}

echo "Checking Level 01 playable wiring from repo files..."

require_file "$scene"
require_file "$trial"
require_text "$trial" "artSceneName: Level_01_HundredSails"
require_text "$trial" "SeaLion.Gameplay.Levels.Level01TrialRuntime"
require_text "$trial" "SeaLion.UI.Levels.Level01TrialHud"
require_text "$trial" "SeaLion.Presentation.Levels.Level01TrialScenePresenter"

for name in \
  PHASE__Opening_ReferenceMatch \
  PHASE__Traversal_GateRescue_ReferenceMatch \
  PHASE__BeachLanding_ReferenceMatch \
  PHASE__BossBattle_Prototype_NoExecutionReference \
  PHASE__VictoryReward_Prototype_NoExecutionReference \
  PLAYER__Flagship \
  GATE__Multiplier_x4 \
  RESCUE__CaptiveSailmakers \
  BOSS__HarborGuardian \
  CHARACTER__Hayreddin_OnDeck \
  FRIENDLY__LandingForce \
  HOSTILE__Defenders_Front \
  CITY__MountainBackdrop \
  ENV__LeftCoastalCliff \
  GROUP__LandingFortress_Right
do
  require_text "$scene" "$name"
done

for key in sailToShore holdBeach clearDefenders strikeGuardian engage gatePending landing
do
  require_text "$copy" "\"$key\""
done

for asset in \
  "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.fbx" \
  "Assets/_Project/Art/Ships/L01-SHP-002_Landing_Craft_Optimized.fbx" \
  "Assets/_Project/Art/Characters/L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx" \
  "Assets/_Project/Art/Characters/L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx" \
  "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx" \
  "Assets/_Project/Art/Environment/L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx" \
  "Assets/_Project/Art/Environment/L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx" \
  "Assets/_Project/Art/Environment/L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx" \
  "Assets/_Project/Art/Environment/L01-ENV-015_Fortress_R6_Modular_R5_VISIBLE_REVIEW.fbx" \
  "Assets/_Project/Art/Environment/L01-PRP-004_Captive_Sailmakers_Rescue_Raft_Cage_Optimized.fbx"
do
  require_guid_in_scene "$asset"
done

if (( fail == 0 )); then
  echo "[level01] playable wiring check passed"
  exit 0
fi
echo "[level01] playable wiring check failed" >&2
exit 1
