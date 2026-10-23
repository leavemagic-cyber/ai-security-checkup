#!/usr/bin/env node
import { readFileSync, realpathSync, existsSync, statSync } from 'node:fs';
import { delimiter, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

export const catalog = JSON.parse(readFileSync(new URL('./advisories.json', import.meta.url), 'utf8'));

// Prereleases are deliberately not treated as patched stable versions.
export function parseVersion(value) {
  if (typeof value !== 'string') return null;
  const match = /^v?(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:\+[0-9A-Za-z.-]+)?$/.exec(value);
  if (!match) return null;
  const parts = match.slice(1).map(Number);
  return parts.every(Number.isSafeInteger) ? parts : null;
}

function compare(a, b) {
  for (let i = 0; i < 3; i++) {
    if (a[i] !== b[i]) return a[i] < b[i] ? -1 : 1;
  }
  return 0;
}

export function checkNode(version, location = 'provided version') {
  const parsed = parseVersion(version);
  const base = { component: 'node', version, location, source: catalog.node.source };
  if (!parsed || !catalog.node.minimum[parsed[0]]) {
    return { ...base, status: 'REVIEW', detail: 'Unknown, unlisted or prerelease branch. Check the current official release/support policy; this snapshot cannot clear it.' };
  }
  const minimum = catalog.node.minimum[parsed[0]];
  return {
    ...base,
    status: compare(parsed, parseVersion(minimum)) < 0 ? 'AFFECTED_VERSION' : 'MEETS_BASELINE',
    detail: `Minimum for the 2026-10-21 advisory in this major: ${minimum}. App-bundled copies, running processes and containers need separate checks.`
  };
}

export function checkPackage(name, version, location = 'provided version') {
  const rules = catalog.packages.filter(rule => rule.name === name);
  if (!rules.length) return [];
  const parsed = parseVersion(version);
  return rules.map(rule => {
    let status = 'NO_VERSION_MATCH';
    if (!parsed || (rule.review_below && compare(parsed, parseVersion(rule.review_below)) < 0)) {
      status = 'REVIEW';
    } else if (rule.intervals.some(([lower, upper]) => compare(parsed, parseVersion(lower)) >= 0 && compare(parsed, parseVersion(upper)) < 0)) {
      status = 'AFFECTED_VERSION';
    } else if (rule.manual_after_upgrade && rule.intervals.some(([, upper]) => compare(parsed, parseVersion(upper)) >= 0)) {
      // A lockfile cannot establish correct credential migration or instance isolation.
      status = 'MANUAL_CHECK';
    }
    return { component: name, version, location, advisory: rule.id, status, detail: rule.fix, conditions: rule.conditions, source: rule.source };
  });
}

export function readLockfile(lock) {
  if (!lock || typeof lock !== 'object' || Array.isArray(lock)) throw new Error('Invalid lockfile object');
  const found = [];
  if ([2, 3].includes(lock.lockfileVersion) && lock.packages && typeof lock.packages === 'object' && !Array.isArray(lock.packages)) {
    for (const [location, row] of Object.entries(lock.packages)) {
      if (!row || typeof row !== 'object' || Array.isArray(row)) throw new Error(`Invalid package record: ${location}`);
      if (!location) continue;
      const marker = 'node_modules/';
      const index = location.lastIndexOf(marker);
      if (index < 0) continue;
      if (row.name !== undefined && typeof row.name !== 'string') throw new Error(`Invalid package name: ${location}`);
      const name = row.name || location.slice(index + marker.length);
      found.push(...checkPackage(name, row.version ?? '(unresolved link)', location));
    }
  } else if (lock.lockfileVersion === 1 && lock.dependencies && typeof lock.dependencies === 'object' && !Array.isArray(lock.dependencies)) {
    const visit = (dependencies, parent = '') => {
      if (!dependencies || typeof dependencies !== 'object' || Array.isArray(dependencies)) throw new Error('Invalid dependency records');
      for (const [name, row] of Object.entries(dependencies)) {
        if (!row || typeof row !== 'object' || Array.isArray(row)) throw new Error(`Invalid package record: ${name}`);
        const location = `${parent}node_modules/${name}`;
        found.push(...checkPackage(name, row.version ?? '(unresolved)', location));
        if (row.dependencies) visit(row.dependencies, `${location}/`);
      }
    };
    visit(lock.dependencies);
  } else {
    throw new Error('Unsupported or incomplete lockfile: require npm package-lock v1, v2 or v3. No safety conclusion is possible.');
  }
  return found;
}

export function pathCandidates(envPath = process.env.PATH ?? '') {
  const candidates = new Set();
  for (const directory of envPath.split(delimiter)) {
    if (!directory) continue;
    const candidate = join(directory.replace(/^"|"$/g, ''), process.platform === 'win32' ? 'node.exe' : 'node');
    try { if (existsSync(candidate)) candidates.add(realpathSync(candidate)); } catch { /* Inventory only; no claim about unreadable paths. */ }
  }
  return [...candidates];
}

export function exitCode(report) {
  if (report.errors.length || report.findings.some(f => ['REVIEW', 'MANUAL_CHECK'].includes(f.status))) return 2;
  return report.findings.some(f => f.status === 'AFFECTED_VERSION') ? 1 : 0;
}

export function run(args) {
  const options = { project: null, nodeVersion: process.version, components: [], json: false };
  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    if (arg === '--json') options.json = true;
    else if (arg === '--help' || arg === '-h') return { help: true };
    else if (['--project', '--node-version', '--component'].includes(arg)) {
      const value = args[++i];
      if (!value || value.startsWith('--')) throw new Error(`Missing value for ${arg}`);
      if (arg === '--project') options.project = value;
      if (arg === '--node-version') options.nodeVersion = value;
      if (arg === '--component') options.components.push(value);
    } else throw new Error(`Unknown option: ${arg}`);
  }
  const report = {
    catalogDate: catalog.checked_at,
    checkedAt: new Date().toISOString(),
    scope: catalog.scope,
    pathCandidates: pathCandidates(),
    findings: [checkNode(options.nodeVersion, args.includes('--node-version') ? 'provided version; not a runtime observation' : process.execPath)],
    errors: [],
    limitations: [
      'Offline snapshot, not a complete vulnerability scanner or intrusion detector.',
      'Only the executing Node runtime, explicitly supplied versions, and an optional npm lockfile are evaluated.',
      'Other PATH candidates are listed but never executed. No desktop, running-process, container, remote-host, pnpm, yarn or Python inventory is automatic.',
      'Lockfiles describe intended resolution, not the installed or running deployment. Exposure conditions and post-upgrade migrations require manual review.',
      'No network, installation, configuration changes, credential reads or uploads. Output may contain local paths; review before sharing.'
    ]
  };
  for (const value of options.components) {
    const separator = value.lastIndexOf('@');
    const name = value.slice(0, separator);
    const version = value.slice(separator + 1);
    if (separator <= 0 || !version) throw new Error('Use --component PACKAGE@VERSION');
    const findings = checkPackage(name, version);
    if (!findings.length) throw new Error(`Package not covered by this catalog: ${name}`);
    report.findings.push(...findings);
  }
  if (options.project) {
    const file = resolve(options.project, 'package-lock.json');
    try {
      // Bound memory use of user-supplied inputs before reading. Raise this only
      // after reviewing larger lockfiles and the available memory budget.
      if (statSync(file).size > 25 * 1024 * 1024) throw new Error('Lockfile exceeds the 25 MiB input limit');
      const data = readFileSync(file, 'utf8').replace(/^\uFEFF/, '');
      if (Buffer.byteLength(data) > 25 * 1024 * 1024) throw new Error('Lockfile exceeds the 25 MiB input limit');
      report.findings.push(...readLockfile(JSON.parse(data)));
    } catch (error) {
      report.errors.push({ file, message: error.message });
    }
  }
  return { report, json: options.json };
}

function main() {
  try {
    const result = run(process.argv.slice(2));
    if (result.help) {
      console.log('node check.mjs [--project DIR] [--component PACKAGE@VERSION] [--node-version VERSION] [--json]\nRead-only and offline. Exit 0: no listed version flags; 1: affected version; 2: incomplete/manual review.');
      return;
    }
    const { report } = result;
    if (result.json) console.log(JSON.stringify(report, null, 2));
    else {
      console.log(`AI Security Checkup | catalog ${report.catalogDate} | selected advisories only`);
      for (const finding of report.findings) {
        console.log(`\n[${finding.status}] ${finding.component} ${finding.version}${finding.advisory ? ` (${finding.advisory})` : ''}\n  ${finding.location}\n  ${finding.detail}\n  ${finding.source}`);
      }
      for (const error of report.errors) console.error(`[INCOMPLETE] ${error.file}: ${error.message}`);
      console.log('\nOther PATH candidates (versions NOT evaluated):\n' + report.pathCandidates.join('\n'));
      console.log('\n' + report.limitations.join('\n'));
    }
    process.exitCode = exitCode(report);
  } catch (error) {
    console.error(`Check failed: ${error.message}`);
    process.exitCode = 2;
  }
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) main();
