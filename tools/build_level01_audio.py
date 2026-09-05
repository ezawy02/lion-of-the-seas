#!/usr/bin/env python3
"""Build the lightweight Level 1 audio review set from licensed source recordings."""

from __future__ import annotations

import json
import shutil
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "ArtSource/Audio/Incoming/OpenGameArt/Level01"
OUTPUT = ROOT / "Assets/_Project/Audio/Level01"
REVIEW = ROOT / "Artifacts/Local/Approval/Level01Audio/FullPass_R1"


def require_tool(name: str) -> None:
    if shutil.which(name) is None:
        raise RuntimeError(f"Required tool is unavailable: {name}")


def run(command: list[str]) -> None:
    subprocess.run(command, cwd=ROOT, check=True)


def source(relative: str) -> Path:
    value = SOURCE / relative
    if not value.is_file():
        raise FileNotFoundError(value)
    return value


def render(
    name: str,
    inputs: list[Path],
    filter_complex: str,
    output_label: str = "out",
    quality: int = 6,
) -> Path:
    target = OUTPUT / name
    command = ["ffmpeg", "-hide_banner", "-loglevel", "error", "-y"]
    for input_path in inputs:
        command.extend(("-i", str(input_path)))
    command.extend(
        (
            "-filter_complex",
            filter_complex,
            "-map",
            f"[{output_label}]",
            "-ar",
            "48000",
        )
    )
    if target.suffix == ".ogg":
        command.extend(("-c:a", "vorbis", "-strict", "experimental", "-q:a", str(quality)))
    elif target.suffix == ".mp3":
        command.extend(("-c:a", "libmp3lame", "-q:a", "2"))
    elif target.suffix == ".wav":
        command.extend(("-c:a", "pcm_s16le"))
    else:
        raise ValueError(f"Unsupported output format: {target.suffix}")
    command.append(str(target))
    run(command)
    return target


def build_broadside() -> Path:
    cannon = source("BattleAtSea/cannon_fire_CC0_Thimras.ogg")
    hull = source("BattleAtSea/ship_ram_ship_shortened_CC0_Thimras.ogg")
    filters = (
        "[0:a]atrim=start=0:end=3.25,asetpts=PTS-STARTPTS,highpass=f=30,"
        "equalizer=f=95:t=q:w=0.85:g=2.4,equalizer=f=210:t=q:w=1:g=1.6,"
        "equalizer=f=3000:t=q:w=1.2:g=-1.4,"
        "acompressor=threshold=0.20:ratio=1.8:attack=3:release=240:makeup=1.05[cannon];"
        "[1:a]atrim=start=0.20:end=1.28,asetpts=PTS-STARTPTS,highpass=f=95,"
        "lowpass=f=4300,equalizer=f=260:t=q:w=1.1:g=2,"
        "afade=t=in:st=0:d=0.025,afade=t=out:st=0.62:d=0.46,"
        "volume=0.18,adelay=45|58[hull];"
        "aevalsrc='0.18*sin(2*PI*(58-10*t)*t)*exp(-6.8*t)':s=48000:d=0.58,"
        "pan=stereo|c0=c0|c1=c0,highpass=f=32,lowpass=f=140,adelay=18|25[sub];"
        "[cannon][hull][sub]amix=inputs=3:duration=longest:normalize=0,"
        "equalizer=f=160:t=q:w=0.9:g=1.2,"
        "acompressor=threshold=0.28:ratio=1.55:attack=2:release=180:makeup=1,"
        "volume=1.55,alimiter=limit=0.891:attack=1:release=80:level=disabled,"
        "afade=t=out:st=2.75:d=0.48[out]"
    )
    return render("L01_SFX_Broadside_Cannon_R3.ogg", [cannon, hull], filters, quality=7)


def build_gate() -> list[Path]:
    energy = source("Gate/movingshield_sound_CC0_zeroisnotnull.ogg")
    loop_filters = (
        "[0:a]highpass=f=95,lowpass=f=9000,equalizer=f=820:t=q:w=1:g=1.8,"
        "loudnorm=I=-25:LRA=7:TP=-2[out]"
    )
    gate_loop = render("L01_SFX_Gate_EnergyLoop_R1.wav", [energy], loop_filters)

    pulse_filters = (
        "[0:a]asplit=4[p1i][p2i][p3i][p4i];"
        "[p1i]atrim=0.06:0.64,asetpts=PTS-STARTPTS,highpass=f=130,lowpass=f=10500,"
        "afade=t=out:st=0.34:d=0.24,volume=0.48[p1];"
        "[p2i]atrim=0.06:0.64,asetpts=PTS-STARTPTS,asetrate=46690,aresample=48000,"
        "highpass=f=140,lowpass=f=10800,afade=t=out:st=0.32:d=0.22,"
        "volume=0.52,adelay=360|360[p2];"
        "[p3i]atrim=0.06:0.64,asetpts=PTS-STARTPTS,asetrate=49490,aresample=48000,"
        "highpass=f=150,lowpass=f=11200,afade=t=out:st=0.30:d=0.20,"
        "volume=0.56,adelay=700|700[p3];"
        "[p4i]atrim=0.02:0.86,asetpts=PTS-STARTPTS,asetrate=55560,aresample=48000,"
        "highpass=f=160,lowpass=f=11800,afade=t=out:st=0.46:d=0.30,"
        "volume=0.64,adelay=1010|1010[p4];"
        "sine=f=440:r=48000:d=0.30,afade=t=out:st=0.06:d=0.24,volume=0.035[t1];"
        "sine=f=554.37:r=48000:d=0.30,afade=t=out:st=0.06:d=0.24,"
        "volume=0.035,adelay=360|360[t2];"
        "sine=f=659.25:r=48000:d=0.30,afade=t=out:st=0.06:d=0.24,"
        "volume=0.038,adelay=700|700[t3];"
        "sine=f=880:r=48000:d=0.55,afade=t=out:st=0.08:d=0.47,"
        "volume=0.045,adelay=1010|1010[t4];"
        "[p1][p2][p3][p4][t1][t2][t3][t4]amix=inputs=8:duration=longest:normalize=0,"
        "highpass=f=90,equalizer=f=2300:t=q:w=1:g=1.5,"
        "loudnorm=I=-17:LRA=8:TP=-1.2[out]"
    )
    multiply = render("L01_SFX_Gate_MultiplyX4_R1.ogg", [energy], pulse_filters, quality=7)
    return [gate_loop, multiply]


def build_landing() -> Path:
    water = source("Water/water_drops_CC0_angeloyazar.ogg")
    hull = source("BattleAtSea/ship_ram_ship_shortened_CC0_Thimras.ogg")
    filters = (
        "[0:a]atrim=0:2.35,asetpts=PTS-STARTPTS,highpass=f=65,lowpass=f=10000,"
        "equalizer=f=620:t=q:w=1:g=1.8,volume=0.82[water];"
        "[1:a]atrim=0.20:1.55,asetpts=PTS-STARTPTS,highpass=f=90,lowpass=f=4600,"
        "equalizer=f=240:t=q:w=1:g=2.2,afade=t=out:st=0.72:d=0.60,"
        "volume=0.18,adelay=105|120[hull];"
        "aevalsrc='0.10*sin(2*PI*(72-12*t)*t)*exp(-7*t)':s=48000:d=0.55,"
        "pan=stereo|c0=c0|c1=c0,lowpass=f=170,adelay=70|78[sub];"
        "[water][hull][sub]amix=inputs=3:duration=longest:normalize=0,"
        "loudnorm=I=-18:LRA=9:TP=-1.2,afade=t=out:st=1.88:d=0.44[out]"
    )
    return render("L01_SFX_Landing_ShallowWater_R1.ogg", [water, hull], filters, quality=7)


def build_damage_and_loss() -> list[Path]:
    """Create short, readable failure cues from the already licensed local source set."""
    energy = source("Gate/movingshield_sound_CC0_zeroisnotnull.ogg")
    wall = source("BattleAtSea/cannon_hit_wall_no_splash_CC0_Thimras.ogg")
    hull = source("BattleAtSea/ship_ram_ship_shortened_CC0_Thimras.ogg")
    water = source("Water/water_drops_CC0_angeloyazar.ogg")

    gate_filters = (
        "[0:a]atrim=0.03:0.95,asetpts=PTS-STARTPTS,asetrate=52000,aresample=48000,"
        "highpass=f=110,lowpass=f=6200,afade=t=out:st=0.52:d=0.34,volume=0.40[energy];"
        "[1:a]atrim=0.02:0.92,asetpts=PTS-STARTPTS,asetrate=40800,aresample=48000,"
        "highpass=f=55,lowpass=f=3300,equalizer=f=180:t=q:w=0.9:g=2.4,"
        "afade=t=out:st=0.50:d=0.38,volume=0.48,adelay=65|78[impact];"
        "aevalsrc='0.12*sin(2*PI*(120-68*t)*t)*exp(-5.5*t)':s=48000:d=0.72,"
        "pan=stereo|c0=c0|c1=c0,lowpass=f=210,adelay=40|46[warning];"
        "[energy][impact][warning]amix=inputs=3:duration=longest:normalize=0,"
        "loudnorm=I=-18:LRA=7:TP=-1.4,afade=t=out:st=0.78:d=0.22[out]"
    )
    gate_damage = render(
        "L01_SFX_Gate_Damage_R1.ogg", [energy, wall], gate_filters, quality=6
    )

    loss_filters = (
        "[0:a]atrim=0.18:1.32,asetpts=PTS-STARTPTS,asetrate=42400,aresample=48000,"
        "highpass=f=65,lowpass=f=3600,equalizer=f=240:t=q:w=1:g=2.2,"
        "afade=t=out:st=0.68:d=0.42,volume=0.54[hull];"
        "[1:a]atrim=0.02:0.88,asetpts=PTS-STARTPTS,highpass=f=150,lowpass=f=6800,"
        "afade=t=out:st=0.48:d=0.34,volume=0.24,adelay=125|155[splash];"
        "aevalsrc='0.09*sin(2*PI*(98-42*t)*t)*exp(-4.8*t)':s=48000:d=0.82,"
        "pan=stereo|c0=c0|c1=c0,lowpass=f=190,adelay=35|45[fall];"
        "[hull][splash][fall]amix=inputs=3:duration=longest:normalize=0,"
        "loudnorm=I=-19:LRA=8:TP=-1.5,afade=t=out:st=0.92:d=0.25[out]"
    )
    crew_loss = render("L01_SFX_Crew_Loss_R1.ogg", [hull, water], loss_filters, quality=6)
    return [gate_damage, crew_loss]


def build_guardian() -> list[Path]:
    metal = source("BattleAtSea/cannon_hit_cannon_CC0_Thimras.ogg")
    wall = source("BattleAtSea/cannon_hit_wall_no_splash_CC0_Thimras.ogg")
    hull = source("BattleAtSea/ship_ram_ship_shortened_CC0_Thimras.ogg")

    hit_filters = (
        "[0:a]atrim=0:1.32,asetpts=PTS-STARTPTS,asetrate=43200,aresample=48000,"
        "highpass=f=55,lowpass=f=9300,equalizer=f=420:t=q:w=1:g=2.2,"
        "equalizer=f=2600:t=q:w=1.2:g=1.4,afade=t=out:st=0.92:d=0.42,volume=0.88[metal];"
        "[1:a]atrim=0.20:0.88,asetpts=PTS-STARTPTS,highpass=f=75,lowpass=f=2400,"
        "afade=t=out:st=0.35:d=0.32,volume=0.10,adelay=18|24[body];"
        "aevalsrc='0.15*sin(2*PI*(78-18*t)*t)*exp(-9*t)':s=48000:d=0.42,"
        "pan=stereo|c0=c0|c1=c0,lowpass=f=190[sub];"
        "[metal][body][sub]amix=inputs=3:duration=longest:normalize=0,"
        "loudnorm=I=-16:LRA=7:TP=-1.2[out]"
    )
    hit = render("L01_SFX_Guardian_ArmorHit_R1.ogg", [metal, hull], hit_filters, quality=7)

    defeat_filters = (
        "[0:a]atrim=0:1.63,asetpts=PTS-STARTPTS,highpass=f=42,lowpass=f=8500,"
        "equalizer=f=150:t=q:w=0.9:g=2.6,volume=0.88[stone1];"
        "[0:a]atrim=0:1.45,asetpts=PTS-STARTPTS,asetrate=36000,aresample=48000,"
        "lowpass=f=5000,afade=t=out:st=0.95:d=0.55,volume=0.35,adelay=560|610[stone2];"
        "[1:a]atrim=0:1.75,asetpts=PTS-STARTPTS,asetrate=38400,aresample=48000,"
        "highpass=f=50,lowpass=f=7200,afade=t=out:st=1.28:d=0.50,"
        "volume=0.52,adelay=140|185[armor];"
        "[2:a]atrim=0.28:2.70,asetpts=PTS-STARTPTS,highpass=f=70,lowpass=f=4300,"
        "afade=t=out:st=1.45:d=0.85,volume=0.22,adelay=310|345[collapse];"
        "aevalsrc='0.13*sin(2*PI*(56-12*t)*t)*exp(-2.3*t)':s=48000:d=2.6,"
        "pan=stereo|c0=c0|c1=c0,lowpass=f=125,adelay=80|95[rumble];"
        "[stone1][stone2][armor][collapse][rumble]amix=inputs=5:duration=longest:normalize=0,"
        "acompressor=threshold=0.24:ratio=1.7:attack=4:release=260:makeup=1,"
        "loudnorm=I=-16:LRA=10:TP=-1.1,afade=t=out:st=2.72:d=0.46[out]"
    )
    defeat = render(
        "L01_SFX_Guardian_Defeat_R1.ogg", [wall, metal, hull], defeat_filters, quality=7
    )
    return [hit, defeat]


def build_results() -> list[Path]:
    win = source("Music/win_fretless_CC0_Fupi.ogg")
    defeat = source("Music/medieval_defeat_CC0_RandomMind.mp3")
    reward_filters = (
        "[0:a]atrim=0:3.72,asetpts=PTS-STARTPTS,highpass=f=85,lowpass=f=13500,"
        "equalizer=f=330:t=q:w=0.9:g=1.6,equalizer=f=1750:t=q:w=1:g=1.2,"
        "acompressor=threshold=0.22:ratio=1.45:attack=8:release=180:makeup=1,"
        "loudnorm=I=-17:LRA=8:TP=-1.2,afade=t=out:st=3.30:d=0.40[out]"
    )
    reward = render("L01_SFX_Reward_Corsair_R1.ogg", [win], reward_filters, quality=7)
    failure_filters = (
        "[0:a]atrim=0:6.25,asetpts=PTS-STARTPTS,highpass=f=45,lowpass=f=12500,"
        "equalizer=f=170:t=q:w=1:g=1.8,loudnorm=I=-20:LRA=8:TP=-1.5,"
        "afade=t=out:st=5.35:d=0.88[out]"
    )
    failure = render("L01_SFX_Failure_Medieval_R1.mp3", [defeat], failure_filters, quality=6)
    return [reward, failure]


def build_ambience_and_music() -> list[Path]:
    sea = source("Water/wave_03_CC0_jasinski.flac")
    wind = source("Water/wind_whoosh_loop_CC0_SketchMan3.ogg")
    traversal = source("Music/pirate_ship_theme_CC0_beardalaxy.ogg")
    battle = source("Music/final_battle_CC0_skrjablin.ogg")

    sea_clip = render(
        "L01_AMB_SeaLoop_R1.ogg",
        [sea],
        "[0:a]atrim=0:1.98,asetpts=PTS-STARTPTS,highpass=f=45,lowpass=f=10500,"
        "equalizer=f=380:t=q:w=1:g=-1.2,asplit=3[bodyin][tailin][headin];"
        "[bodyin]atrim=0.35:1.63,asetpts=PTS-STARTPTS[body];"
        "[tailin]atrim=1.63:1.98,asetpts=PTS-STARTPTS[tail];"
        "[headin]atrim=0:0.35,asetpts=PTS-STARTPTS[head];"
        "[tail][head]acrossfade=d=0.35:c1=qsin:c2=qsin[seam];"
        "[body][seam]concat=n=2:v=0:a=1,loudnorm=I=-31:LRA=6:TP=-4[out]",
        quality=5,
    )
    wind_clip = render(
        "L01_AMB_WindLoop_R1.wav",
        [wind],
        "[0:a]highpass=f=115,lowpass=f=7800,equalizer=f=1100:t=q:w=1:g=-1.5,"
        "loudnorm=I=-34:LRA=6:TP=-5[out]",
        quality=5,
    )
    traversal_clip = render(
        "L01_MUS_Traversal_Pirate_R1.mp3",
        [traversal],
        "[0:a]highpass=f=38,lowpass=f=15500,loudnorm=I=-25:LRA=10:TP=-2[out]",
        quality=6,
    )
    battle_clip = render(
        "L01_MUS_GuardianBattle_R1.mp3",
        [battle],
        "[0:a]highpass=f=35,lowpass=f=16000,loudnorm=I=-23:LRA=11:TP=-2[out]",
        quality=5,
    )
    return [sea_clip, wind_clip, traversal_clip, battle_clip]


def probe(path: Path) -> dict[str, object]:
    command = [
        "ffprobe",
        "-v",
        "error",
        "-show_entries",
        "format=duration,size,bit_rate:stream=codec_name,sample_rate,channels",
        "-of",
        "json",
        str(path),
    ]
    value = json.loads(subprocess.check_output(command, cwd=ROOT, text=True))
    stream = value["streams"][0]
    media = value["format"]
    return {
        "path": path.relative_to(ROOT).as_posix(),
        "durationSeconds": round(float(media["duration"]), 3),
        "bytes": int(media["size"]),
        "bitRate": int(media.get("bit_rate", 0)),
        "codec": stream["codec_name"],
        "sampleRate": int(stream["sample_rate"]),
        "channels": int(stream["channels"]),
    }


def main() -> None:
    require_tool("ffmpeg")
    require_tool("ffprobe")
    OUTPUT.mkdir(parents=True, exist_ok=True)
    REVIEW.mkdir(parents=True, exist_ok=True)

    built = [build_broadside()]
    built.extend(build_gate())
    built.append(build_landing())
    built.extend(build_damage_and_loss())
    built.extend(build_guardian())
    built.extend(build_results())
    built.extend(build_ambience_and_music())

    metrics = {"revision": "Level01Audio_R1_REVIEW", "clips": [probe(path) for path in built]}
    (REVIEW / "audio_metrics.json").write_text(
        json.dumps(metrics, indent=2, ensure_ascii=False) + "\n", encoding="utf-8"
    )
    total = sum(item["bytes"] for item in metrics["clips"])
    print(f"Built {len(built)} Level 1 clips ({total / (1024 * 1024):.2f} MiB).")


if __name__ == "__main__":
    main()
