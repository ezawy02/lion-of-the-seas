#!/usr/bin/env python3
"""Verify production scene entry points, script/asset links and reward targets from YAML."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parent.parent


def read(path):
    return (ROOT / path).read_text()


def guid(path):
    return re.search(r'^guid: (\w+)', read(str(path) + '.meta'), re.M)[1]


def require(condition, message):
    if not condition:
        raise AssertionError(message)


def main():
    build = read('ProjectSettings/EditorBuildSettings.asset')
    paths = re.findall(r'enabled: 1\n\s+path: (.+)', build)
    require(paths[0] == 'Assets/_Project/Scenes/Bootstrap.unity', 'Bootstrap must be first.')
    require('directLevelScene: Level_01_Playable_Trial' in read(paths[0]), 'Bootstrap target is not playable.')
    known_guids = {re.search(r'^guid: (\w+)', p.read_text(), re.M)[1]
                   for p in (ROOT / 'Assets').rglob('*.meta')
                   if re.search(r'^guid: (\w+)', p.read_text(), re.M)}
    # Existing Unity UI/Input System and built-in resource GUIDs in the authored shell.
    package_guids = {'ca9f5fa95ffab41fb9a615ab714db018', '01614664b831546d2ae94a42149d80ac',
                     '76c392e42b5098c458856cdf6ecaaaa1', '0000000000000000f000000000000000',
                     '0000000000000000e000000000000000'}
    ids = set()
    for p in (ROOT / 'Assets/_Project/Data/Loadouts/VerticalSlice').glob('*.asset'):
        match = re.search(r'\n  id:\s*(?:\{value: ([^}]+)\}|\n\s+value: ([^\n]+))', p.read_text())
        if match:
            ids.add((match[1] or match[2]).strip())
    for number, art in [(1, 'Level_01_HundredSails'), (2, 'Level_02_ChainStrait'), (3, 'Level_03_StormFortress')]:
        shell = f'Assets/_Project/Scenes/Trials/Level_0{number}_Playable_Trial.unity'
        require(shell in paths, f'{shell} is not in build settings.')
        content = read(shell)
        require(f'Assets/_Project/Scenes/{art}.unity' in paths, f'{art} is unavailable to additive loading.')
        require('artSceneName: ' + art in content, f'{shell} points to the wrong art scene.')
        for script in ['Gameplay/Levels/Level01TrialRuntime', 'UI/Levels/Level01TrialHud', 'UI/Levels/Level01LoadoutFlow',
                       'Presentation/Levels/' + ('Level01TrialScenePresenter' if number == 1 else 'CampaignScenePresenter')]:
            require(guid('Assets/_Project/Scripts/' + script + '.cs') in content, f'Missing {script} in {shell}.')
        require(guid(f'Assets/_Project/Data/Levels/Level0{number}/Level0{number}.asset') in content,
                f'{shell} has no matching level definition.')
        unknown = set(re.findall(r'guid: (\w+)', content)) - known_guids - package_guids
        require(not unknown, f'{shell} has unresolved new GUIDs: {unknown}')
        declarations = set(re.findall(r'^--- !u!\d+ &(-?\d+)', content, re.M))
        for component in re.findall(r'component: \{fileID: (-?\d+)\}', content):
            require(component in declarations, f'{shell} has missing component {component}.')
        reward = read(f'Assets/_Project/Data/Rewards/Level0{number}Blueprint.asset')
        target = re.search(r'grantTargetId:\s*\n\s+value: ([^\n]+)', reward)[1]
        require(target in ids, f'Reward unlock target is undefined: {target}')
        print(f'PASS level {number}: build entry, art, scripts, definition, component IDs, reward target')
    print('Scene-link checks passed. This does not execute or visually approve scenes.')


if __name__ == '__main__':
    main()
