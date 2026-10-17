#!/usr/bin/env python3
"""Offline structure/syntax-shape smoke checks. NOT a Unity/C# compile test."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PRJ = ROOT / 'src' / 'client-unity'
REQUIRED = [
    'Assets/Scripts/Domain/Definitions.cs',
    'Assets/Scripts/Domain/EntityState.cs',
    'Assets/Scripts/Domain/GameWorld.cs',
    'Assets/Scripts/Runtime/PrototypeGame.cs',
    'Assets/Editor/DomainSelfTests.cs',
    'Assets/Editor/CreateArenaScene.cs',
    'Packages/manifest.json',
    'ProjectSettings/ProjectVersion.txt',
    'README.md',
]


def balanced(source):
    # Lightweight lexer for braces, comments, strings and character literals.
    stack, state, i = [], 'code', 0
    while i < len(source):
        c = source[i]
        n = source[i + 1] if i + 1 < len(source) else ''
        if state == 'code':
            if c == '/' and n == '/':
                state = 'line'; i += 1
            elif c == '/' and n == '*':
                state = 'block'; i += 1
            elif c == '"':
                state = 'string'
            elif c == "'":
                state = 'char'
            elif c in '{([':
                stack.append(c)
            elif c in '})]':
                if not stack or stack.pop() != {'}':'{',')':'(',']':'['}[c]:
                    return False
        elif state == 'line':
            if c == '\n': state = 'code'
        elif state == 'block':
            if c == '*' and n == '/': state = 'code'; i += 1
        elif state in ('string','char'):
            if c == '\\': i += 1
            elif (c == '"' and state == 'string') or (c == "'" and state == 'char'): state = 'code'
        i += 1
    return not stack and state in ('code','line')


for relative in REQUIRED:
    full = PRJ / relative
    assert full.is_file(), f'Missing {relative}'
    if full.suffix == '.cs':
        txt = full.read_text(encoding='utf8')
        assert balanced(txt), f'Unbalanced tokens: {relative}'
        if 'Domain/' in relative:
            assert 'using UnityEngine;' not in txt and 'UnityEngine.' not in txt, f'Gameplay domain must not reference UnityEngine: {relative}'

manifest = json.loads((PRJ/'Packages/manifest.json').read_text(encoding='utf8'))
assert isinstance(manifest['dependencies'], dict)
world = (PRJ/'Assets/Scripts/Domain/GameWorld.cs').read_text(encoding='utf8')
for requirement in ['Submit(', 'Tick(', 'DealDamage(', 'GrantExperience(', 'Purchase(', 'CastAbility(', 'ApplyForm(', 'ProtectedCore(']:
    assert requirement in world, f'Missing gameplay API: {requirement}'
print(f'STATIC PASS: {len(REQUIRED)} required files, balanced token shapes, JSON, domain isolation, core API.')
print('NOT CHECKED: Unity C# compiler, Play Mode, performance, networking, artwork licensing.')
