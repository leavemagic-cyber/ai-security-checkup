import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { checkNode, checkPackage, readLockfile, parseVersion, run, exitCode } from '../check.mjs';

for (const [older, fixed] of [['22.23.3', '22.23.4'], ['24.22.0', '24.22.1'], ['26.12.0', '26.12.1']]) {
  test(`Node boundary ${fixed}`, () => {
    assert.equal(checkNode(older).status, 'AFFECTED_VERSION');
    assert.equal(checkNode(fixed).status, 'MEETS_BASELINE');
  });
}
test('Numeric comparison handles multi-digit minor versions', () => {
  assert.equal(checkNode('24.9.9').status, 'AFFECTED_VERSION');
  assert.equal(checkNode('24.100.0').status, 'MEETS_BASELINE');
});
test('Unlisted/EOL branches and prereleases never receive a passing baseline', () => {
  for (const value of ['20.20.0', '25.9.0', '27.0.0', '24.22.1-rc.1', 'garbage', '024.22.1']) assert.equal(checkNode(value).status, 'REVIEW');
  assert.deepEqual(parseVersion('v24.22.1+vendor.1'), [24, 22, 1]);
});
test('n8n maintenance patch is accepted while 2.44.0 remains flagged', () => {
  for (const version of ['2.42.9', '2.43.4', '2.44.0']) assert.equal(checkPackage('n8n', version)[0].status, 'AFFECTED_VERSION');
  for (const version of ['2.43.5', '2.43.9', '2.44.1']) assert.equal(checkPackage('n8n', version)[0].status, 'NO_VERSION_MATCH');
  assert.equal(checkPackage('n8n', '1.123.85')[0].status, 'REVIEW');
});
test('OAuth and shared-server migration cannot be cleared from a lockfile', () => {
  assert.equal(checkPackage('@modelcontextprotocol/client', '2.2.0')[0].status, 'MANUAL_CHECK');
  assert.equal(checkPackage('@modelcontextprotocol/server', '2.3.0')[0].status, 'MANUAL_CHECK');
  assert.equal(checkPackage('@modelcontextprotocol/client', '1.0.0')[0].status, 'NO_VERSION_MATCH');
});
test('npm v3 finds nested copies, including an alias with its actual package name', () => {
  const rows = readLockfile({ lockfileVersion: 3, packages: {
    '': { name: 'unrelated', version: '1.0.0' },
    'node_modules/other/node_modules/@modelcontextprotocol/client': { version: '2.1.0' },
    'node_modules/alias': { name: '@anthropic-ai/claude-code', version: '2.1.178' }
  } });
  assert.equal(rows.length, 2);
  assert.ok(rows.every(row => row.status === 'AFFECTED_VERSION'));
});
test('npm v1 recursively finds vulnerable transitive packages', () => {
  const rows = readLockfile({ lockfileVersion: 1, dependencies: { other: { version: '1.0.0', dependencies: { n8n: { version: '2.44.0' } } } } });
  assert.equal(rows[0].location, 'node_modules/other/node_modules/n8n');
  assert.equal(rows[0].status, 'AFFECTED_VERSION');
});
test('unresolved links, missing/invalid lockfiles and unsupported formats are incomplete', () => {
  assert.equal(readLockfile({ lockfileVersion: 2, packages: { 'node_modules/n8n': { link: true } } })[0].status, 'REVIEW');
  assert.throws(() => readLockfile({}), /Unsupported/);
  assert.throws(() => readLockfile({ lockfileVersion: 3, packages: [] }), /Unsupported/);
  assert.throws(() => readLockfile({ lockfileVersion: 3, packages: { 'node_modules/n8n': null } }), /Invalid/);
  assert.throws(() => readLockfile({ lockfileVersion: 1, dependencies: { n8n: { dependencies: 'bad' } } }), /Invalid/);
  const directory = mkdtempSync(join(tmpdir(), 'ai-checkup-test-'));
  try {
    let result = run(['--node-version', '24.22.1', '--project', directory]);
    assert.equal(exitCode(result.report), 2);
    writeFileSync(join(directory, 'package-lock.json'), '{broken');
    result = run(['--node-version', '24.22.1', '--project', directory]);
    assert.equal(exitCode(result.report), 2);
  } finally { rmSync(directory, { recursive: true }); }
});
test('typos and uncovered packages fail explicitly', () => {
  assert.throws(() => run(['--project']), /Missing value/);
  assert.throws(() => run(['--component', 'unknown@1.0.0']), /not covered/);
  assert.throws(() => run(['--force']), /Unknown option/);
});
test('CLI emits parseable JSON with a nonzero vulnerable-version result', () => {
  const child = spawnSync(process.execPath, ['check.mjs', '--node-version', '24.22.0', '--json'], { cwd: new URL('..', import.meta.url), encoding: 'utf8' });
  assert.equal(child.status, 1, child.stderr);
  assert.equal(JSON.parse(child.stdout).findings[0].status, 'AFFECTED_VERSION');
});
