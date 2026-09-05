#!/usr/bin/env python3
"""Compile authored C# and optional managed domain tests without starting Unity or Blender.
Uses an installed Unity distribution's compiler and reference assemblies only. This does
not import assets, run Unity lifecycle tests, build a player, or certify device performance.
"""
import argparse
import json
import os
from pathlib import Path
import subprocess


def invoke_compiler(scripting, refs, sources, output, unsafe=True):
    args = ['-nologo', '-target:library', '-langversion:latest', '-nostdlib+',
            '-define:UNITY_EDITOR,UNITY_INCLUDE_TESTS', '-nowarn:0649,0169,0414', '-out:' + str(output)]
    if unsafe:
        args.append('-unsafe')
    args += ['-r:' + str(p) for p in dict.fromkeys(refs)] + [str(p) for p in sources]
    response = output.with_suffix('.rsp')
    response.write_text('\n'.join('"' + arg + '"' for arg in args))
    subprocess.run([str(scripting / 'NetCoreRuntime/dotnet'),
                    str(scripting / 'DotNetSdkRoslyn/csc.dll'), '@' + str(response)], check=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity-resources', type=Path, required=True)
    parser.add_argument('--package-assemblies', type=Path)
    parser.add_argument('--output', type=Path, default=Path('Artifacts/Local/source-check'))
    parser.add_argument('--run-domain-tests', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parent.parent
    os.chdir(root)
    base = args.unity_resources.resolve()
    scripting = base / 'Scripting'
    packages = args.package_assemblies
    if packages is None:
        candidates = sorted((base / 'PackageManager/ProjectTemplates/libcache').glob(
            'com.unity.template.3d-cross-platform-*/ScriptAssemblies'))
        if not candidates:
            parser.error('Provide --package-assemblies with the installed package reference DLL directory.')
        packages = candidates[-1]
    nunit_dir = base / 'PackageManager/BuiltInPackages/com.unity.ext.nunit/net40/unity-custom'
    refs = list((scripting / 'NetStandard/ref/2.1.0').glob('*.dll'))
    refs += list((scripting / 'NetStandard/compat/2.1.0/shims/netfx').glob('*.dll'))
    refs += [p for p in (scripting / 'Managed/UnityEngine').glob('*.dll') if not p.name.startswith('UnityEditor')]
    refs.append(scripting / 'Managed/UnityEditor.dll')
    refs.append(nunit_dir / 'nunit.framework.dll')
    for name in ['Unity.Mathematics', 'Unity.Burst', 'Unity.Collections', 'Unity.InputSystem',
                 'UnityEngine.UI', 'Unity.RenderPipelines.Core.Runtime', 'Unity.RenderPipelines.Universal.Runtime',
                 'UnityEngine.TestRunner', 'UnityEditor.TestRunner', 'Unity.PerformanceTesting']:
        refs.append(packages / (name + '.dll'))
    missing = [str(p) for p in refs if not p.is_file()]
    if missing:
        parser.error('Missing reference assemblies: ' + ', '.join(missing))
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    sources = []
    for directory in ['Scripts', 'VFX', 'Editor', 'Tests']:
        sources += sorted((Path('Assets/_Project') / directory).rglob('*.cs'))
    aggregate = output / 'SeaLion.AllSourceCheck.dll'
    invoke_compiler(scripting, refs, sources, aggregate)
    print('PASS aggregate C# compilation:', len(sources), 'files', flush=True)
    definitions = {}
    for path in Path('Assets/_Project').rglob('*.asmdef'):
        definition = json.loads(path.read_text())
        definitions[definition['name']] = (path, definition)
    groups = {name: [] for name in definitions}
    for source in sources:
        owners = [(name, path.parent) for name, (path, _) in definitions.items()
                  if source.is_relative_to(path.parent)]
        if owners:
            groups[max(owners, key=lambda pair: len(pair[1].parts))[0]].append(source)
    completed = {}
    remaining = set(definitions)
    while remaining:
        ready = sorted(name for name in remaining if all(
            ref not in definitions or ref in completed for ref in definitions[name][1].get('references', [])))
        if not ready:
            raise RuntimeError('Assembly dependency cycle: ' + str(remaining))
        for name in ready:
            definition = definitions[name][1]
            dependencies = [completed[ref] for ref in definition.get('references', []) if ref in completed]
            assembly = output / (name + '.dll')
            invoke_compiler(scripting, refs + dependencies, groups[name], assembly,
                            definition.get('allowUnsafeCode', False))
            completed[name] = assembly
            remaining.remove(name)
            print('PASS assembly:', name, flush=True)
    if args.run_domain_tests:
        response = output / 'domain.rsp'
        executable = output / 'DomainRunner.exe'
        options = ['-nologo', '-target:exe', '-langversion:latest', '-nostdlib+', '-out:' + str(executable),
                   '-r:' + str(aggregate)] + ['-r:' + str(p) for p in dict.fromkeys(refs)]
        options += ['tools/SourceDomainChecks.cs']
        response.write_text('\n'.join('"' + value + '"' for value in options))
        subprocess.run([str(scripting / 'NetCoreRuntime/dotnet'),
                        str(scripting / 'DotNetSdkRoslyn/csc.dll'), '@' + str(response)], check=True)
        env = dict(os.environ, MONO_PATH=os.pathsep.join(map(str, [output, scripting / 'Managed/UnityEngine',
                                                                 packages, nunit_dir, scripting / 'Managed'])))
        subprocess.run([str(scripting / 'MonoBleedingEdge/bin/mono'), str(executable), str(aggregate)],
                       env=env, check=True)
    print('Unity/Blender processes were not started by this checker. Asset import and device checks remain pending.')


if __name__ == '__main__':
    main()
