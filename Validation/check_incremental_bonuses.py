"""Run the actual bonus maths and per-projectile history outside Unity (macOS Hub install)."""
import argparse
import json
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--editor-root', help='Unity.app/Contents directory')
args = parser.parse_args()
version = re.search(r'm_EditorVersion: (.+)', (ROOT/'ProjectSettings/ProjectVersion.txt').read_text())[1].strip()
editor = Path(args.editor_root) if args.editor_root else Path('/Applications/Unity/Hub/Editor')/version/'Unity.app/Contents'
base = editor/'Resources/Scripting'
runtime = base/'NetCoreRuntime'
shared = max((runtime/'shared/Microsoft.NETCore.App').iterdir(), key=lambda p: tuple(int(n) for n in p.name.split('.')))
compiler = next((base/'DotNetSdk/sdk').glob('*/Roslyn/bincore/csc.dll'))
with tempfile.TemporaryDirectory(prefix='kotlety-incremental-') as directory:
    temp = Path(directory)
    # Owner lookup requires Unity's native component runtime. Stub only that boundary;
    # the arithmetic and target/distance history below compile directly from project sources.
    stub = temp/'OwnerStub.cs'
    stub.write_text('namespace RetyKotlety { public class PlayerUpgrades : UnityEngine.MonoBehaviour { public CombatBonuses Bonuses=>default; } }')
    references = list(shared.glob('*.dll'))+[base/'Managed/UnityEngine/UnityEngine.CoreModule.dll']
    sources = [ROOT/'Assets/Scripts/Combat/CombatBonuses.cs', ROOT/'Assets/Scripts/Combat/AttackDamage.cs', ROOT/'Validation/IncrementalBonusChecks.cs', ROOT/'Validation/ProjectileFanChecks.cs', ROOT/'Assets/Scripts/Combat/ProjectileFan.cs', ROOT/'Assets/Scripts/Combat/PanBowlingStats.cs', stub]
    response = temp/'checks.rsp'
    response.write_text('\n'.join(['-nologo','-target:exe','-out:'+str(temp/'Checks.dll')]+['-r:'+str(p) for p in references]+[str(p) for p in sources]))
    subprocess.run([str(runtime/'dotnet'),str(compiler),'@'+str(response)],check=True)
    (temp/'Checks.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net8.0','framework':{'name':'Microsoft.NETCore.App','version':shared.name}}}))
    for name in ['UnityEngine.CoreModule.dll','UnityEngine.SharedInternalsModule.dll']:
        source = base/'Managed/UnityEngine'/name
        if source.exists(): shutil.copy2(source,temp/name)
    result = subprocess.run([str(runtime/'dotnet'),str(temp/'Checks.dll')],text=True,capture_output=True)
    print(result.stdout,end='')
    if result.stderr: print(result.stderr,end='')
    result.check_returncode()
    reports = ROOT/'Validation/Reports'
    reports.mkdir(exist_ok=True)
    (reports/'incremental-bonus-math-results.txt').write_text(result.stdout+'Scope: actual bonus and hit-history calculations; owner-component lookup and Unity Play Mode not run.\n')
