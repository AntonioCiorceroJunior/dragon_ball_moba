#!/usr/bin/env python3
"""Validador offline da documentação e do contrato mínimo de trabalho."""
from pathlib import Path
import sys

root=Path(__file__).resolve().parents[1]
required=[
    'AGENTS.md','README.md','docs/specifications/GAME_DESIGN.md',
    'docs/specifications/CODEX_WORKFLOW.md','docs/specifications/TECH_ARCHITECTURE.md',
    'docs/coordination/README.md','docs/coordination/TO_CODEX.md',
    'docs/coordination/PROJECT_STATUS.md','docs/coordination/FROM_CODEX.md',
    'docs/ADR/ADR-001-ENGINE-AND-REUSE.md','legal/ASSET_REGISTER.csv'
]
missing=[name for name in required if not (root/name).is_file()]
if missing:
    print('ERRO: arquivos ausentes: '+', '.join(missing)); sys.exit(1)
text=(root/'docs/specifications/GAME_DESIGN.md').read_text(encoding='utf-8')
if len(text)<90000 or '# 32.' not in text or '# 33.' not in text:
    print('ERRO: documento mestre parece incompleto'); sys.exit(1)
agent=(root/'AGENTS.md').read_text(encoding='utf-8')
if len(agent)>3000:
    print('AVISO: AGENTS.md pode estar longo demais para contexto econômico')
status=(root/'docs/coordination/PROJECT_STATUS.md').read_text(encoding='utf-8')
if 'DBM-000' not in status:
    print('AVISO: status atual não referencia DBM-000')
print(f'OK: {len(required)} arquivos essenciais presentes; GDD = {len(text)} caracteres; contexto básico consistente.')
