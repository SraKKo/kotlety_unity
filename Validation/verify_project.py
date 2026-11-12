"""Check Unity asset references; optionally compile using the editor's Bee configuration."""
import argparse
import re
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent


def check_assets():
    guids = {}
    for meta in (ROOT / 'Assets').rglob('*.meta'):
        assert meta.with_suffix('').exists(), f'Orphan meta: {meta}'
        match = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(), re.M)
        assert match, f'Missing GUID: {meta}'
        guid = match[1]
        assert guid not in guids, f'Duplicate GUID: {meta} / {guids.get(guid)}'
        guids[guid] = meta
    for path in (ROOT / 'Assets').rglob('*'):
        if path.name.startswith('.') or path.suffix == '.meta':
            continue
        assert Path(str(path) + '.meta').exists(), f'Missing meta: {path}'
    for scene in (ROOT / 'Assets').rglob('*.unity'):
        for guid in re.findall(r'm_Script: \{[^}]*guid: ([0-9a-f]{32})', scene.read_text()):
            assert guid in guids, f'Missing script {guid}: {scene}'
    print(f'PASS: {len(guids)} unique asset GUIDs; scene script references resolve.')


def compile_project(editor_root):
    version = re.search(r'm_EditorVersion: (.+)', (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text())[1].strip()
    editor = Path(editor_root) if editor_root else Path('/Applications/Unity/Hub/Editor') / version / 'Unity.app/Contents'
    scripting = editor / 'Resources/Scripting'
    dotnet = scripting / 'NetCoreRuntime/dotnet'
    compilers = list((scripting / 'DotNetSdk/sdk').glob('*/Roslyn/bincore/csc.dll'))
    assert dotnet.exists() and compilers, 'Cannot find compiler; pass --editor-root pointing to Unity.app/Contents.'
    sources = {}
    for source in (ROOT / 'Assets').rglob('*.cs'):
        sources.setdefault(source.name, []).append(source.relative_to(ROOT).as_posix())
    with tempfile.TemporaryDirectory(prefix='kotlety-check-') as temp:
        output = Path(temp)
        # Compile dependencies in order without overwriting assemblies used by the open editor.
        configurations = list((ROOT / 'Library/Bee/artifacts').glob('*/Assembly-CSharp.rsp'))
        assert configurations, 'Open the project in Unity first to generate Library/Bee compiler configuration.'
        config = max(configurations, key=lambda p: p.stat().st_mtime).parent
        for assembly in ('Assembly-CSharp', 'Assembly-CSharp-Editor'):
            response = config / (assembly + '.rsp')
            text = response.read_text(encoding='utf-8-sig')
            for name in ('Assembly-CSharp', 'Assembly-CSharp-Editor'):
                for extension in ('.dll', '.ref.dll'):
                    text = text.replace((config / (name + extension)).relative_to(ROOT).as_posix(), str(output / (name + extension)))
            def source_path(match):
                path = match[1]
                if (ROOT / path).exists():
                    return match[0]
                candidates = sources.get(Path(path).name, [])
                assert len(candidates) == 1, f'Cannot resolve relocated source: {path}'
                return '"' + candidates[0] + '"'
            text = re.sub(r'^"(Assets/[^"\n]+\.cs)"$', source_path, text, flags=re.M)
            included = set(re.findall(r'^"(Assets/[^"\n]+\.cs)"$', text, re.M))
            source_root = ROOT / ('Assets/Editor' if assembly.endswith('-Editor') else 'Assets/Scripts')
            for source in sorted(source_root.rglob('*.cs')):
                relative = source.relative_to(ROOT).as_posix()
                if relative not in included:
                    text += '\n"' + relative + '"' 
            rsp = output / response.name
            rsp.write_text(text)
            subprocess.run([str(dotnet), str(compilers[-1]), '-nologo', '@' + str(rsp)], cwd=ROOT, check=True)
            print(f'PASS: {assembly} compiled.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compile', action='store_true', help='Compile runtime and editor C# with installed Unity.')
    parser.add_argument('--editor-root', help='Unity.app/Contents directory for the installed editor.')
    args = parser.parse_args()
    check_assets()
    if args.compile:
        compile_project(args.editor_root)
